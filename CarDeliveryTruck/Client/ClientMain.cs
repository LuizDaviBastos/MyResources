using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Mono.CSharp;
using static CitizenFX.Core.Native.API;

namespace CarDeliveryTruck.Client
{
    public class ClientMain : BaseScript
    {
        public List<Vehicle> VehiclesInTask { get; set; } = new List<Vehicle>();
        public Vector3 From = new Vector3(2133.2f, 4783.3f, 40.9f);
        public Vector3 To = new Vector3(2133.2f, 4783.3f, 40.9f);
        public Ped Driver { get; set; }
        public int Car { get; set; }
        public Vehicle CarLocal { get; set; }
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
                        TriggerEvent("open:rampa", Driver.Handle, Car);

                        Debug.WriteLine($"VehiclesInTask: {VehiclesInTask.Count}");

                    }), false);

                    RegisterCommand("detach", new Action<int, List<object>, string>(async (source, args, raw) =>
                    {
                        foreach (var vehicle in VehiclesInTask)
                        {
                            CarLocal.Detach();
                        }
                    }), false);


                    RegisterCommand("truck", new Action<int, List<object>, string>(async (source, args, raw) =>
                    {
                        var vehicles = await SpawnTruck(args.Count > 0 ? args[0]?.ToString() : "");

                        Car = VehToNet(vehicles.Value.Handle);
                        CarLocal = vehicles.Value;
                        DecorSetInt(vehicles.Key.Handle, "flatbed3_car", Car);
                        DecorSetBool(vehicles.Key.Handle, "flatbed3_attached", true);
                        Driver = await World.CreatePed(PedHash.PrologueHostage01, vehicles.Key.Position, vehicles.Key.Heading);
                        Driver.SetIntoVehicle(vehicles.Key, VehicleSeat.Driver);

                        VehiclesInTask.Add(vehicles.Key);

                        if (GetWaypointCoords(out Vector3 coords))
                        {
                            TaskVehicleDriveToCoordLongrange(Driver.Handle, vehicles.Key.Handle, coords.X, coords.Y, coords.Z, 10, 1, 30);
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
            if (VehiclesInTask.Any())
            {
                if (GetWaypointCoords(out Vector3 coords))
                {
                    foreach (var vehicle in VehiclesInTask)
                    {
                        Debug.WriteLine($"Waypoint Coords: X: {coords.X} Y: {coords.Y} Z: {coords.Z}");
                        Debug.WriteLine($"Vehicle Coords: X: {vehicle.Position.X} Y: {vehicle.Position.Y} Z: {vehicle.Position.Z}");
                        Debug.WriteLine($"Distance: {GetDistance(coords, vehicle.Position)}");


                        if (GetDistance(coords, vehicle.Position) <= 51)
                        {
                            TriggerEvent("open:rampa", Driver.Handle, Car);
                            // int en = vehicle.Handle;
                            //DeleteVehicle(ref en);
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

            if (name == "flatbed3")
            {
                Car = VehToNet(veh.Handle);

                if (!HasModelLoaded(new Model("inm_flatbed_base")))
                {
                    RequestModel(new Model("inm_flatbed_base"));
                }

                int bedNet;
                Vector3 a = Vector3.Zero, pos = Vector3.Zero;
                GetEntityMatrix(veh.Handle, ref a, ref a, ref a, ref pos);
                DecorSetInt(veh.Handle, "flatbed3_bed", 0);
                var bed = CreateObjectNoOffset(new Model("inm_flatbed_base"), pos.X, pos.Y, pos.Z, true, false, false);
                var bedEntity = Entity.FromHandle(bed);
                bedEntity.AttachTo(veh, new Vector3(0, -3.5f, 0.4f));
                //log("GENERATING BED")
                if (DoesEntityExist(bed))
                {
                    bedNet = ObjToNet(bed);
                    DecorSetInt(veh.Handle, "flatbed3_bed", bedNet);
                    //log("DONE GENERATING BED")
                }

                veh2.AttachTo(bedEntity, new Vector3(0, 2, 0.7f));

                DecorSetInt(veh.Handle, "flatbed3_car", Car);
                DecorSetBool(veh.Handle, "flatbed3_attached", true);
                DecorSetInt(veh.Handle, "flatbed3_state", 1);
            }
            else
            {
                veh2.AttachTo(veh, new Vector3(0, -2, 1));
            }

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