﻿using Microsoft.Extensions.Caching.Distributed;
using MvcCoreElastiCacheAWS.Helpers;
using MvcCoreElastiCacheAWS.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MvcCoreElastiCacheAWS.Services
{
    public class ServiceAWSCache
    {
        private IDistributedCache cache;
        

        public ServiceAWSCache(IDistributedCache cache)
        {
            this.cache = cache;
        }



        public async Task<List<Coche>> GetCochesFavoritosAsync()
        {
            //se supone que podriamos tener coches almacenados
            //mediante una key
            //almacenaremos los coches utilizando json y en una coleccion
            string jsonCoches = await this.cache.GetStringAsync("cochesfavoritos");
            if(jsonCoches == null)
            {
                return null;
            }else
            {
                List<Coche> cars = JsonConvert.DeserializeObject<List<Coche>>(jsonCoches);
                return cars;
            }
        }


        public async Task AddCocheAsync(Coche car)
        {
            //preguntar si existen coches o no todavia
            List<Coche> coches = await this.GetCochesFavoritosAsync();
            //si no devueleve nada, es la primera vez que almacenamos algo..
            //y creamos la coleccion
            if(coches == null)
            {
                coches = new List<Coche>();
            }
            //añadimos el nuevo coche favorito
            coches.Add(car);
            //serializamos a json
            string jsonCoches = JsonConvert.SerializeObject(coches);
            //almacenamos con la key de redis
            await this.cache.SetStringAsync("cochesfavoritos", jsonCoches, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));

        }

        public async Task DeleteCocheFavoritoAsync(int idcoche)
        {
            List<Coche> cars = await this.GetCochesFavoritosAsync();
            if (cars != null)
            {
                Coche carEliminar = cars.FirstOrDefault(x => x.IdCoche == idcoche);
                cars.Remove(carEliminar);
                //comprobamos si ya no existen coches favoritos
                if (cars.Count == 0)
                {
                    await this.cache.RemoveAsync("cochesfavoritos");
                }
                else
                {
                    //SERIALIZAMOS Y ALMACENAMOS LA COLECCION ACTUALIZADA
                    string jsonCoches =
                        JsonConvert.SerializeObject(cars);
                    await this.cache.SetStringAsync("cochesfavoritos"
                        , jsonCoches, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
                }
            }

        }
    }
}
