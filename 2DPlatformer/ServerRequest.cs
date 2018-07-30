using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectPlatformer.Character;
using ProjectPlatformer.Time;
using NetworkCommsDotNet;
using NetworkCommsDotNet.DPSBase;
using SharpZipLibCompressor;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Tools;
using NetworkCommsDotNet.Connections.TCP;
using ProtoBuf;

namespace ProjectPlatformer
{
    [ProtoContract]
    public class ServerRequest
    {
        [ProtoMember(1)]
        public RequestType request;

        public ServerRequest(RequestType requestType)
        {
            request = requestType;
        }
    }
    [ProtoContract]
    public enum RequestType
    {

    };
}
