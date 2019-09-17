namespace RakDotNet
{
    public enum MessageIdentifier : byte
    {
        // These types are never returned to the user.
        // Ping from a connected system.  Update timestamps (internal use only)
        InternalPing,

        // Ping from an unconnected system.  Reply but do not update timestamps. (internal use only)
        Ping,

        // Ping from an unconnected system.  Only reply if we have open connections. Do not update timestamps. (internal use only)
        PingOpenConnections,

        // Pong from a connected system.  Update timestamps (internal use only)
        ConnectedPong,

        // Asking for a new connection (internal use only)
        ConnectionRequest,

        // Connecting to a secured server/peer (internal use only)
        SecuredConnectionResponse,

        // Connecting to a secured server/peer (internal use only)
        SecuredConnectionConfirmation,

        // Packet that tells us the packet contains an integer ID to name mapping for the remote system (internal use only)
        RpcMapping,

        // A reliable packet to detect lost connections (internal use only)
        DetectLostConnections,

        // Offline message so we know when to reset and start a new connection (internal use only)
        OpenConnectionRequest,

        // Offline message response so we know when to reset and start a new connection (internal use only)
        OpenConnectionReply,

        // Remote procedure call (internal use only)
        Rpc,

        // Remote procedure call reply, for RPCs that return data (internal use only)
        RpcReply,

        // RakPeer - Same as ADVERTISE_SYSTEM, but intended for internal use rather than being passed to the user. Second byte indicates type. Used currently for NAT punchthrough for receiver port advertisement. See NAT_ADVERTISE_RECIPIENT_PORT
        OutOfBandInternal,


        //
        // USER TYPES - DO NOT CHANGE THESE
        //

        // RakPeer - In a client/server environment, our connection request to the server has been accepted.
        ConnectionRequestAccepted,

        // RakPeer - Sent to the player when a connection request cannot be completed due to inability to connect. 
        ConnectionAttemptFailed,

        // RakPeer - Sent a connect request to a system we are currently connected to.
        AlreadyConnected,

        // RakPeer - A remote system has successfully connected.
        NewIncomingConnection,

        // RakPeer - The system we attempted to connect to is not accepting new connections.
        NoFreeIncomingConnections,

        // RakPeer - The system specified in Packet::systemAddress has disconnected from us.  For the client, this would mean the server has shutdown. 
        DisconnectionNotification,

        // RakPeer - Reliable packets cannot be delivered to the system specified in Packet::systemAddress.  The connection to that system has been closed. 
        ConnectionLost,

        // RakPeer - We preset an RSA public key which does not match what the system we connected to is using.
        RsaPublicKeyMismatch,

        // RakPeer - We are banned from the system we attempted to connect to.
        ConnectionBanned,

        // RakPeer - The remote system is using a password and has refused our connection because we did not set the correct password.
        InvalidPassword,

        // RakPeer - A packet has been tampered with in transit.  The sender is contained in Packet::systemAddress.
        ModifiedPacket,

        // RakPeer - The four bytes following this byte represent an unsigned int which is automatically modified by the difference in system times between the sender and the recipient. Requires that you call SetOccasionalPing.
        Timestamp,

        // RakPeer - Pong from an unconnected system.  First byte is PONG, second sizeof(RakNetTime) bytes is the ping, following bytes is system specific enumeration data.
        Pong,

        // RakPeer - Inform a remote system of our IP/Port, plus some offline data
        AdvertiseSystem,

        // ConnectionGraph plugin - In a client/server environment, a client other than ourselves has disconnected gracefully.  Packet::systemAddress is modified to reflect the systemAddress of this client.
        RemoteDisconnectionNotification,

        // ConnectionGraph plugin - In a client/server environment, a client other than ourselves has been forcefully dropped. Packet::systemAddress is modified to reflect the systemAddress of this client.
        RemoteConnectionLost,

        // ConnectionGraph plugin - In a client/server environment, a client other than ourselves has connected.  Packet::systemAddress is modified to reflect the systemAddress of the client that is not connected directly to us. The packet encoding is SystemAddress 1, ConnectionGraphGroupID 1, SystemAddress 2, ConnectionGraphGroupID 2
        RemoteNewIncomingConnection,

        // RakPeer - Downloading a large message. Format is DOWNLOAD_PROGRESS (MessageID), partCount (unsigned int), partTotal (unsigned int), partLength (unsigned int), first part data (length <= MAX_MTU_SIZE). See the three parameters partCount, partTotal and partLength in OnFileProgress in FileListTransferCBInterface.h
        DownloadProgress,

        // FileListTransfer plugin - Setup data
        FileListTransferHeader,

        // FileListTransfer plugin - A file
        FileListTransferFile,

        // DirectoryDeltaTransfer plugin - Request from a remote system for a download of a directory
        DdtDownloadRequest,

        // RakNetTransport plugin - Transport provider message, used for remote console
        TransportString,

        // ReplicaManager plugin - Create an object
        ReplicaManagerConstruction,

        // ReplicaManager plugin - Destroy an object
        ReplicaManagerDestruction,

        // ReplicaManager plugin - Changed scope of an object
        ReplicaManagerScopeChange,

        // ReplicaManager plugin - Serialized data of an object
        ReplicaManagerSerialize,

        // ReplicaManager plugin - New connection, about to send all world objects
        ReplicaManagerDownloadStarted,

        // ReplicaManager plugin - Finished downloading all serialized objects
        ReplicaManagerDownloadComplete,

        // ConnectionGraph plugin - Request the connection graph from another system
        ConnectionGraphRequest,

        // ConnectionGraph plugin - Reply to a connection graph download request
        ConnectionGraphReply,

        // ConnectionGraph plugin - Update edges / nodes for a system with a connection graph
        ConnectionGraphUpdate,

        // ConnectionGraph plugin - Add a new connection to a connection graph
        ConnectionGraphNewConnection,

        // ConnectionGraph plugin - Remove a connection from a connection graph - connection was abruptly lost
        ConnectionGraphConnectionLost,

        // ConnectionGraph plugin - Remove a connection from a connection graph - connection was gracefully lost
        ConnectionGraphDisconnectionNotification,

        // Router plugin - route a message through another system
        RouteAndMulticast,

        // RakVoice plugin - Open a communication channel
        RakvoiceOpenChannelRequest,

        // RakVoice plugin - Communication channel accepted
        RakvoiceOpenChannelReply,

        // RakVoice plugin - Close a communication channel
        RakvoiceCloseChannel,

        // RakVoice plugin - Voice data
        RakvoiceData,

        // Autopatcher plugin - Get a list of files that have changed since a certain date
        AutopatcherGetChangelistSinceDate,

        // Autopatcher plugin - A list of files to create
        AutopatcherCreationList,

        // Autopatcher plugin - A list of files to delete
        AutopatcherDeletionList,

        // Autopatcher plugin - A list of files to get patches for
        AutopatcherGetPatch,

        // Autopatcher plugin - A list of patches for a list of files
        AutopatcherPatchList,

        // Autopatcher plugin - Returned to the user: An error from the database repository for the autopatcher.
        AutopatcherRepositoryFatalError,

        // Autopatcher plugin - Finished getting all files from the autopatcher
        AutopatcherFinishedInternal,
        AutopatcherFinished,

        // Autopatcher plugin - Returned to the user: You must restart the application to finish patching.
        AutopatcherRestartApplication,

        // NATPunchthrough plugin - Intermediary got a request to help punch through a nat
        NatPunchthroughRequest,

        // NATPunchthrough plugin - Intermediary cannot complete the request because the target system is not connected
        NatTargetNotConnected,

        // NATPunchthrough plugin - While attempting to connect, we lost the connection to the target system
        NatTargetConnectionLost,

        // NATPunchthrough plugin - Internal message to connect at a certain time
        NatConnectAtTime,

        // NATPunchthrough plugin - Internal message to send a message (to punch through the nat) at a certain time
        NatSendOfflineMessageAtTime,

        // NATPunchthrough plugin - The facilitator is already attempting this connection
        NatInProgress,

        // LightweightDatabase plugin - Query
        DatabaseQueryRequest,

        // LightweightDatabase plugin - Update
        DatabaseUpdateRow,

        // LightweightDatabase plugin - Remove
        DatabaseRemoveRow,

        // LightweightDatabase plugin - A serialized table.  Bytes 1+ contain the table.  Pass to TableSerializer::DeserializeTable
        DatabaseQueryReply,

        // LightweightDatabase plugin - Specified table not found
        DatabaseUnknownTable,

        // LightweightDatabase plugin - Incorrect password
        DatabaseIncorrectPassword,

        // ReadyEvent plugin - Set the ready state for a particular system
        ReadyEventSet,

        // ReadyEvent plugin - Unset the ready state for a particular system
        ReadyEventUnset,

        // All systems are in state READY_EVENT_SET
        ReadyEventAllSet,

        // ReadyEvent plugin - Request of ready event state - used for pulling data when newly connecting
        ReadyEventQuery,

        // Lobby packets. Second byte indicates type.
        LobbyGeneral,

        // Auto RPC procedure call
        AutoRpcCall,

        // Auto RPC functionName to index mapping
        AutoRpcRemoteIndex,

        // Auto RPC functionName to index mapping, lookup failed. Will try to auto recover
        AutoRpcUnknownRemoteIndex,

        // Auto RPC error code
        // See AutoRPC.h for codes, stored in packet->data[1]
        RpcRemoteError,

        // For the user to use.  Start your first enumeration at this value.
        UserPacketEnum
    }
}