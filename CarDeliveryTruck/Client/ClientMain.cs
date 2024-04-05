using CarDeliveryTruck.Client.Models;
using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace CarDeliveryTruck.Client
{
    public partial class ClientMain : BaseScript
    {
        public Vector3 From = new Vector3(2133.2f, 4783.3f, 40.9f);
        public Vector3 To = new Vector3(2133.2f, 4783.3f, 40.9f);
        public Ped Driver { get; set; }
        public int CarLocalNet { get; set; }
        public Vehicle CarLocal { get; set; }
        public Vehicle Truck { get; set; }
        
        public ClientMain()
        {
            Debug.WriteLine("Hi from CarDeliveryTruck.Client!");
            Tick += async () =>
            {
               //Utils.DrawText3D(Game.PlayerPed.Position, $"{Game.PlayerPed.Position.X} {Game.PlayerPed.Position.Y}");
            };

            EventHandlers["rampa:open:done"] += new Action<dynamic>((_) => CarLocal.Detach());

            EventHandlers["onClientResourceStart"] += new Action<string>(async (resourceName) =>
            {
                if (GetCurrentResourceName() == resourceName)
                {
                    Tick += TaskDriveManager.CheckTaskIsDone;
                    Tick += CheckPedGiveVehicle;
                    TaskDriveManager.OnNextTask += (driveTask) =>
                    {
                        TaskVehicleDriveToCoordLongrange(Driver.Handle, driveTask.Vehicle.Handle, driveTask.To.X, driveTask.To.Y, 0, 10, 1, 50);
                        var blip = AddBlipForEntity(driveTask.Vehicle.Handle);
                        SetBlipDisplay(blip, (int)BlipSprite.RaceCar);
                        //SetBlipFriend(blip, true);
                    };

                    TaskDriveManager.OnTaskDone += (driveTask) =>
                    {
                        ClearPedTasks(driveTask.Driver.Handle);
                        ClearVehicleTasks(driveTask.Vehicle.Handle);
                    };

                    TaskDriveManager.OnAllTasksDone += OpenRampa;

                    RegisterCommand("truck", new Action<int, List<object>, string>(async (source, args, raw) =>
                    {
                        await SpawnTruck(args.Count > 0 ? args[0]?.ToString() : "");

                        //tests
                        if(GetWaypointCoords(out Vector3 coords) || true)
                        {
                            TaskDriveManager.AddTaskTo(new DriveTask(new Vector3(-50f, -1097f, 0f), Truck, Driver, 3f));
                            TaskDriveManager.AddTaskTo(new DriveTask(new Vector3(-60f, -1092f, 0f), Truck, Driver, 2f));
                            //TaskDriveManager.AddTaskTo(new DriveTask(Game.PlayerPed.Position, Truck, Driver));
                            TaskDriveManager.StartNextTask();
                        }
                    }), false);

                    RegisterCommand("cls", new Action<int, List<object>, string>(async (source, args, raw) =>
                    {
                        World.GetAllVehicles().Where(x => Vector3.Distance(x.Position, Game.PlayerPed.Position) < 100).ToList().ForEach((Vehicle vehicle) =>
                        {
                            int veh = vehicle.Handle;
                            DeleteVehicle(ref veh);
                        });
                        TaskDriveManager.ClearTasks();

                    }), false);

                    RegisterCommand("detach", new Action<int, List<object>, string>(async (source, args, raw) =>
                    {
                        CarLocal.Detach();
                    }), false);
                }
            });
        }

       
    }
}