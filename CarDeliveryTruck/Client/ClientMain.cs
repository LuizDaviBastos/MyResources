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
        public Vector3 From = new Vector3(2133.2f, 4783.3f, 40.9f);
        public Vector3 To = new Vector3(2133.2f, 4783.3f, 40.9f);

        public ClientMain()
        {
            Debug.WriteLine("Hi from CarDeliveryTruck.Client!");


            EventHandlers["onClientResourceStart"] += new Action<string>(async (resourceName) =>
            {
                Tick += OnTick;
                if (GetCurrentResourceName() == resourceName)
                {
                    RegisterCommand("opendoor", new Action<int, List<object>, string>((source, args, raw) =>
                    {
                        VehiclesInTask.ForEach(x => {
                            DecorSetInt(x.Handle, "flatbed3_state", 4);
                            x.Doors[VehicleDoorIndex.Hood].Open(); 
                            API.SetVehicleDoorOpen(x.Handle, (int)VehicleDoorIndex.Trunk, false, false);
                        });
                        Debug.WriteLine($"VehiclesInTask: {VehiclesInTask.Count}");
                        
                    }), false);

                    RegisterCommand("truck", new Action<int, List<object>, string>( async (source, args, raw) =>
                    {
                        var vehicles = await SpawnTruck(args.Count > 0 ? args[0]?.ToString() : "");
                        var driver = await World.CreatePed(PedHash.PrologueHostage01, vehicles.Key.Position, vehicles.Key.Heading);
                        driver.SetIntoVehicle(vehicles.Key, VehicleSeat.Driver);
                        VehiclesInTask.Add(vehicles.Key);
                        if (GetWaypointCoords(out Vector3 coords))
                        {
                            TaskVehicleDriveToCoordLongrange(driver.Handle, vehicles.Key.Handle, coords.X, coords.Y, coords.Z, 10, 1, 30);
                            var blip = AddBlipForEntity(vehicles.Key.Handle);
                            SetBlipDisplay(blip, (int)BlipSprite.GarbageTruck);
                            SetBlipFriend(blip, true);
                        }
                    }), false);

                    RegisterCommand("cls", new Action<int, List<object>, string>(async (source, args, raw) =>
                    {
                        VehiclesInTask = new List<Vehicle>();
                        World.GetAllVehicles().Where(x => Vector3.Distance(x.Position, Game.PlayerPed.Position) < 100).ToList().ForEach((Vehicle vehicle) => 
                        {
                            int veh = vehicle.Handle;
                            DeleteVehicle(ref veh);
                        });
                    }), false);
                }
            });
        }

        public async Task OnTick()
        {
            if(VehiclesInTask.Any())
            {
                if(GetWaypointCoords(out Vector3 coords))
                {
                    foreach (var vehicle in VehiclesInTask)
                    {
                        Debug.WriteLine($"Waypoint Coords: X: {coords.X} Y: {coords.Y} Z: {coords.Z}");
                        Debug.WriteLine($"Vehicle Coords: X: {vehicle.Position.X} Y: {vehicle.Position.Y} Z: {vehicle.Position.Z}");
                        Debug.WriteLine($"Distance: {GetDistance(coords, vehicle.Position)}");
                        
                        if (GetDistance(coords, vehicle.Position) <= 51)
                        {
                            Debug.WriteLine("Before Delete entity");
                            int en = vehicle.Handle;
                            DeleteVehicle(ref en);
                            Debug.WriteLine("After Delete entity");
                        }
                        else
                        {
                            Debug.WriteLine($"distance no more then 10");
                        }
                    }
                }
            }
        }

        public async Task<KeyValuePair<Vehicle, Vehicle>> SpawnTruck(string name = null)
        {
            Model model = string.IsNullOrEmpty(name) ? new Model(VehicleHash.Flatbed) : name;
            var veh = await World.CreateVehicle(model, Game.PlayerPed.Position + new Vector3(5, 0, 0), Game.PlayerPed.Heading);
            var veh2 = await World.CreateVehicle(new Model(VehicleHash.Adder), Game.PlayerPed.Position + new Vector3(10, 0, 0), Game.PlayerPed.Heading);
            veh2.AttachTo(veh, new Vector3(0, -2, 1));
            return new KeyValuePair<Vehicle, Vehicle>(veh, veh2);
        }

        public bool GetWaypointCoords(out Vector3 coords)
        {
            var waypoint = GetFirstBlipInfoId(8);
            coords = GetBlipCoords(waypoint);
            return waypoint > 0;
        }

        public float GetDistance(Vector3 v1, Vector3 v2) => GetDistanceBetweenCoords(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, true);
    }
}