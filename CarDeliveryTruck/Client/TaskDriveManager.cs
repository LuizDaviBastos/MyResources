using CarDeliveryTruck.Client.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CarDeliveryTruck.Client
{
    public delegate void NextTaskEvent(DriveTask driveTask);
    public delegate void TaskDriveEvent();

    public class TaskDriveManager
    {
        public static event NextTaskEvent OnNextTask;
        public static event NextTaskEvent OnTaskDone;
        public static event TaskDriveEvent OnAllTasksDone;
        private static Queue<DriveTask> DriveTasks { get; set; } = new Queue<DriveTask>();
        private static DriveTask CurrentTask = null;

        public static void AddTaskTo(DriveTask driveTask) => DriveTasks.Enqueue(driveTask);
        public static void ClearTasks() 
        {
            DriveTasks?.Clear();
        }

        public static void StartNextTask()
        {
            if(DriveTasks?.Any() ?? false)
            {
                Debug.WriteLine("StartNextTask");
                CurrentTask = DriveTasks.Dequeue();
                OnNextTask?.Invoke(CurrentTask);
            }
            else
            {
                CurrentTask = null;
                OnAllTasksDone?.Invoke();
            }
        }

        public static async Task CheckTaskIsDone()
        {
            if (CurrentTask == null)
            {
                Debug.WriteLine("CheckTaskIsDone NOT Has");
                return;
            }

            Debug.WriteLine("CheckTaskIsDone Has");
            float distance = Utils.GetDistance(CurrentTask.To, CurrentTask.Vehicle.Position);
            Debug.WriteLine("Distance: " + distance);
            if (distance <= 1f)
            {
                Debug.WriteLine("Dones with distance: " + distance);
                OnTaskDone?.Invoke(CurrentTask);
                StartNextTask();
            }
        }
    }
}
