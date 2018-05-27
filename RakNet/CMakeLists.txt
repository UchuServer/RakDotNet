CMAKE_MINIMUM_REQUIRED(VERSION 2.8)

PROJECT(RakNetStaticLib)

SET(RAKNET_SOURCES 

AsynchronousFileIO.cpp      FormatString.cpp               RakNetTypes.cpp
AutoRPC.cpp                 FullyConnectedMesh.cpp         RakNetworkFactory.cpp
BitStream.cpp               FunctionThread.cpp             RakPeer.cpp
BitStream_NoTemplate.cpp    Gen_RPC8.cpp                   RakSleep.cpp
CheckSum.cpp                GetTime.cpp                    RakString.cpp
CommandParserInterface.cpp  GridSectorizer.cpp             Rand.cpp
ConnectionGraph.cpp         LightweightDatabaseClient.cpp  ReadyEvent.cpp
ConsoleServer.cpp           LightweightDatabaseCommon.cpp  ReliabilityLayer.cpp
DataBlockEncryptor.cpp      LightweightDatabaseServer.cpp  ReplicaManager2.cpp
DataCompressor.cpp          LinuxStrings.cpp               ReplicaManager.cpp
DirectoryDeltaTransfer.cpp  LogCommandParser.cpp           rijndael.cpp
DS_BytePool.cpp             MessageFilter.cpp              Router.cpp
DS_ByteQueue.cpp            NatPunchthrough.cpp            RPCMap.cpp
DS_HuffmanEncodingTree.cpp  NetworkIDManager.cpp           SHA1.cpp
DS_Table.cpp                NetworkIDObject.cpp            SimpleMutex.cpp
EmailSender.cpp             PacketConsoleLogger.cpp        SocketLayer.cpp
EncodeClassName.cpp         PacketFileLogger.cpp           StringCompressor.cpp
EpochTimeToString.cpp       PacketLogger.cpp               StringTable.cpp
ExtendedOverlappedPool.cpp  PluginInterface.cpp            SystemAddressList.cpp
FileList.cpp                RakMemoryOverride.cpp          TableSerializer.cpp
FileListTransfer.cpp        RakNetCommandParser.cpp        TCPInterface.cpp
FileOperations.cpp          RakNetStatistics.cpp           TelnetTransport.cpp
_FindFirst.cpp              RakNetTransport.cpp            ThreadsafePacketLogger.cpp
RakThread.cpp							SuperFastHash.cpp 								Itoa.cpp
HTTPConnection.cpp
)

set(RAKNET_HEADERS

AsynchronousFileIO.h              Export.h                       RakNetTypes.h
AutopatcherPatchContext.h         ExtendedOverlappedPool.h       RakNetVersion.h
AutopatcherRepositoryInterface.h  FileList.h                     RakNetworkFactory.h
AutoRPC.h                         FileListTransferCBInterface.h  RakPeer.h
BigTypes.h                        FileListTransfer.h             RakPeerInterface.h
BitStream.h                       FileOperations.h               RakSleep.h
BitStream_NoTemplate.h            _FindFirst.h                   RakString.h
CheckSum.h                        FormatString.h                 Rand.h
ClientContextStruct.h             FullyConnectedMesh.h           ReadyEvent.h
CommandParserInterface.h          FunctionThread.h               RefCountedObj.h
ConnectionGraph.h                 Gen_RPC8.h                     ReliabilityLayer.h
ConsoleServer.h                   GetTime.h                      ReplicaEnums.h
DataBlockEncryptor.h              GridSectorizer.h               Replica.h
DataCompressor.h                  InternalPacket.h               ReplicaManager2.h
DirectoryDeltaTransfer.h          LightweightDatabaseClient.h    ReplicaManager.h
DS_BinarySearchTree.h             LightweightDatabaseCommon.h    Rijndael-Boxes.h
DS_BPlusTree.h                    LightweightDatabaseServer.h    Rijndael.h
DS_BytePool.h                     LinuxStrings.h                 Router.h
DS_ByteQueue.h                    LogCommandParser.h             RouterInterface.h
DS_Heap.h                         MessageFilter.h                RPCMap.h
DS_HuffmanEncodingTreeFactory.h   MessageIdentifiers.h           RPCNode.h
DS_HuffmanEncodingTree.h          MTUSize.h                      RSACrypt.h
DS_HuffmanEncodingTreeNode.h      NatPunchthrough.h              SHA1.h
DS_LinkedList.h                   NetworkIDManager.h             SimpleMutex.h
DS_List.h                         NetworkIDObject.h              SimpleTCPServer.h
DS_Map.h                          PacketConsoleLogger.h          SingleProducerConsumer.h
DS_MemoryPool.h                   PacketFileLogger.h             SocketLayer.h
DS_OrderedChannelHeap.h           PacketLogger.h                 StringCompressor.h
DS_OrderedList.h                  PacketPool.h                   StringTable.h
DS_Queue.h                        PacketPriority.h               SystemAddressList.h
DS_QueueLinkedList.h              PluginInterface.h              TableSerializer.h
DS_RangeList.h                    RakAssert.h                    TCPInterface.h
DS_Table.h                        RakMemoryOverride.h            TelnetTransport.h
DS_Tree.h                         RakNetCommandParser.h          ThreadPool.h
DS_WeightedGraph.h                RakNetDefines.h                ThreadsafePacketLogger.h
EmailSender.h                     RakNetStatistics.h             TransportInterface.h
EpochTimeToString.h               RakNetTransport.h              Types.h
RakThread.h											SuperFastHash.h									Itoa.h
HTTPConnection.h									Kbhit.h
)


ADD_LIBRARY(RakNet SHARED ${RAKNET_SOURCES})  # STATIC


INSTALL(TARGETS RakNet
        DESTINATION lib)

INSTALL(FILES ${RAKNET_HEADERS}
        DESTINATION include/raknet)


