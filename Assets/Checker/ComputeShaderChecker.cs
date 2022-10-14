using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DeterministicCalculationChecker
{
    public class ComputeShaderChecker : CheckerBase<ComputeShaderChecker.InputMessage, ComputeShaderChecker.OutputMessage>
    {
        #region Type Define
        
        public struct InputMessage : NetworkMessage
        {
            public List<Vector3> value;
        }
        
        public struct OutputMessage : NetworkMessage
        {
            public List<float> value;
        }

        [Serializable]
        public class Result
        {
            public Vector3 input;
            public List<float> outputs;

            public void Clear()
            {
                input = default;
                outputs = default;
            }
        }
        
        #endregion

        public ComputeShader computeShader;
        public string kernelName = "Compute";

        public int outputNum = 10000;
        public float inputValueScale = 1f;
        

        private GraphicsBuffer _inputBuffer;
        private GraphicsBuffer _outputBuffer;

        public Result nonMatchResult;
        public List<Result> results; 

        [ContextMenu("StartCalc")]
        async void StartCalc()
        {
            var inputs = new List<Vector3>();

            for (var i = 0; i < outputNum; ++i)
            {
                inputs.Add(new Vector3(
                               Random.value,
                               Random.value,
                               Random.value
                           )
                           * inputValueScale
                );
            }


            var inputMessage = new InputMessage() {value = inputs};
            
            var dic = await KickCalcTask(inputMessage);

            var clientOutputs = dic.Values.Select(outputMessage => outputMessage.value).ToList();

            
            results.Clear();
            nonMatchResult.Clear();
            for (var i = 0; i < outputNum; ++i)
            {
                var outputs = clientOutputs.Select(co => co[i]).ToList();
             
                var result = new Result()
                {
                    input = inputs[i],
                    outputs = outputs
                };
                results.Add(result);

                var o = outputs[0];
                for (var j = 1; j < outputs.Count; ++j)
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (o != outputs[j])
                    {
                        nonMatchResult = result;
                        Debug.Log("NonMatch!!");
                        return;
                    }
                }
            }

            Debug.Log($"All Match [{clientOutputs.Count}]clients");
        }

        protected override OutputMessage Calc(InputMessage inputMessage)
        {
            var kernel = computeShader.FindKernel(kernelName);

            int size = outputNum;

            _inputBuffer ??= new GraphicsBuffer(GraphicsBuffer.Target.Structured, size, Marshal.SizeOf<Vector3>());
            _outputBuffer ??= new GraphicsBuffer(GraphicsBuffer.Target.Structured, size, Marshal.SizeOf<float>());
            
            _inputBuffer.SetData(inputMessage.value);
            
            computeShader.SetBuffer(kernel, "InputBuffer", _inputBuffer);
            computeShader.SetBuffer(kernel, "OutputBuffer", _outputBuffer);
            computeShader.Dispatch(kernel, outputNum, 1, 1);


            var dataArray = new float[size];
            _outputBuffer.GetData(dataArray);

            var output = dataArray.ToList();

            return new OutputMessage()
            {
                value = output
            };
        }
    }
}