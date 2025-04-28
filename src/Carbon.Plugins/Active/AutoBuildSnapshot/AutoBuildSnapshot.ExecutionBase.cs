using Cysharp.Threading.Tasks;
using Facepunch;
using System;
using System.Diagnostics;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private abstract class ExecutionBase : Pool.IPooled
    {
        private Stopwatch _processWatch;
        private Stopwatch _frameWatch;
        private int _frameSteps;

        /// <summary>
        /// The time it took to process the snapshot, or the current running duration if not yet completed.
        /// </summary>
        public double Duration => _processWatch.Elapsed.TotalMilliseconds;

        /// <summary>
        /// The time it took to process the last frame.
        /// </summary>
        public double FrameDuration => _frameWatch.Elapsed.TotalMilliseconds;

        /// <summary>
        /// The number of frames it took to process the snapshot.
        /// </summary>
        public int YieldCount { get; private set; }

        /// <summary>
        /// The exception, if any, that was thrown during processing.
        /// </summary>
        public Exception Exception { get; private set; }

        public virtual void EnterPool()
        {
            _processWatch.Reset();
            _frameWatch.Reset();

            Exception = null;
        }

        public virtual void LeavePool()
        {
            _processWatch ??= new();
            _frameWatch ??= new();
            _frameSteps = 0;

            YieldCount = 0;
        }

        public async UniTaskVoid BeginTask()
        {
            _processWatch.Start();
            try
            {
                await ProcessAsync();
            }
            catch (Exception ex)
            {
                Exception = ex;
                _instance.AddLogMessage($"Error during snapshot processing: {ex.Message}");
            }
            finally
            {
                _processWatch.Stop();
            }
        }

        protected abstract UniTask ProcessAsync();

        protected async UniTask YieldReset()
        {
            _processWatch.Stop();
            await UniTask.Yield();
            _processWatch.Start();

            _frameSteps = 0;
            _frameWatch.Restart();
            YieldCount++;
        }

        protected async UniTask YieldStep(int maxSteps = 50)
        {
            if (++_frameSteps > maxSteps
                || FrameDuration > _config.Advanced.MaxStepDuration)
            {
                await YieldReset();
            }
        }
    }
}
