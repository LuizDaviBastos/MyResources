using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace CarDeliveryTruck.Client
{
    public class ClientMain : BaseScript
    {
        public List<Vehicle> VehiclesInTask { get; set; } = new List<Vehicle>();
        public ClientMain()
        {
            Debug.WriteLine("Hi from CarDeliveryTruck.Client!");
        }

        [Tick]
        public async Task OnTick()
        {
            if(IsControlJustPressed(0, (int)Control.Pickup))
            {
                var v = await SpawnTruck();
                var driver = await World.CreatePed(PedHash.Abigail, v.Key.Position, v.Key.Heading);
                driver.SetIntoVehicle(v.Key, VehicleSeat.Driver);

                if(GetWaypointCoords(out Vector3 coords))
                {
                    TaskVehicleDriveToCoordLongrange(driver.Handle, v.Key.Handle, coords.X, coords.Y, coords.Z, 10, 1, 30);
                    VehiclesInTask.Add(v.Key);

                    var blip = AddBlipForEntity(v.Key.Handle);
                    SetBlipFriend(blip, true);
                }
            }

            if(VehiclesInTask.Any())
            {
                if(GetWaypointCoords(out Vector3 coords))
                {
                    foreach (var vehicle in VehiclesInTask)
                    {
                        if(Vector3.Distance(coords, vehicle.Position) < 10)
                        {
                            int en = vehicle.Handle;
                            DeleteVehicle(ref en);
                            Debug.WriteLine("Delete entity");
                        }
                    }
                }
            }
        }

        public async Task<KeyValuePair<Vehicle, Vehicle>> SpawnTruck()
        {
            var veh = await World.CreateVehicle(new Model(VehicleHash.Flatbed), Game.PlayerPed.Position + new Vector3(5, 0, 0), Game.PlayerPed.Heading);
            var veh2 = await World.CreateVehicle(new Model(VehicleHash.Adder), Game.PlayerPed.Position + new Vector3(10, 0, 0), Game.PlayerPed.Heading);
            // veh2.AttachTo(veh, new Vector3(0, -2, 1));
            return new KeyValuePair<Vehicle, Vehicle>(veh, veh2);
        }

        public bool GetWaypointCoords(out Vector3 coords)
        {
            var waypoint = GetFirstBlipInfoId(8);
            coords = GetBlipCoords(waypoint);
            return waypoint > 0;
        }
    }
}