using System.Linq;
using Mirror;
using UnityEngine;

namespace DeterministicCalculationChecker
{
    public class ComputeShaderChecker : CheckerBase<ComputeShaderChecker.InputMessage, ComputeShaderChecker.OutputMessage>
    {
        public struct InputMessage : NetworkMessage
        {
            public Vector3 value;
        }
        
        public struct OutputMessage : NetworkMessage
        {
            public float value;
        }

        public Vector3 input;
        
        [ContextMenu("StartCalc")]
        async void StartCalc()
        {
            var inputMessage = new InputMessage() {value = input};
            
            var dic = await KickCalcTask(inputMessage);
            
            Debug.Log(string.Join("\n", dic.Select(pair => $"({pair.Key} {pair.Value.value})").ToArray()));
        }

        protected override OutputMessage Calc(InputMessage inputMessage)
        {
            var output = Vector3.Magnitude(inputMessage.value);

            return new OutputMessage()
            {
                value = output
            };
        }
    }
}