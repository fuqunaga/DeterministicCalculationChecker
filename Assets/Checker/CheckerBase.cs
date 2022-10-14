using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

namespace DeterministicCalculationChecker
{
    public abstract class CheckerBase<TInputMessage, TOutputMessage> : NetworkBehaviour
        where TInputMessage : struct, NetworkMessage
        where TOutputMessage : struct, NetworkMessage
    {
        private readonly Dictionary<int, TOutputMessage> _connIdOutputTable = new();

        #region Server
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            NetworkServer.RegisterHandler<TOutputMessage>(OnReceiveOutput);
        }
        
        private void OnReceiveOutput(NetworkConnectionToClient conn, TOutputMessage outputMessage)
        {
            _connIdOutputTable[conn.connectionId] = outputMessage;
        }
        
        public async Task<Dictionary<int, TOutputMessage>> KickCalcTask(TInputMessage inputMessage)
        {
            _connIdOutputTable.Clear();

            var connectionCount = NetworkServer.connections.Count;
            NetworkServer.SendToAll(inputMessage);

            await Task.Run(() =>
            {
                while (_connIdOutputTable.Count < connectionCount)
                {}
            });
            
            return _connIdOutputTable;
        }
        
        
        #endregion


        #region Client

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            NetworkClient.RegisterHandler<TInputMessage>(OnReceiveInput);
        }

        private void OnReceiveInput(TInputMessage inputMessage)
        {
            var outputMessage = Calc(inputMessage);

            NetworkClient.Send(outputMessage);
        }


        protected abstract TOutputMessage Calc(TInputMessage inputMessage);

        #endregion
    }
}