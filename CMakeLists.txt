cmake_minimum_required(VERSION 2.8)

project(RakDotNetNative CXX)

include_directories(".")

add_subdirectory("RakNet")

set(RAKDOTNET_SOURCES
    Native/BitStream.cpp
    Native/RakNetTypes.cpp
    Native/RakNetworkFactory.cpp
    Native/RakPeerInterface.cpp
)

set(LIBS
    RakNet
    pthread
)

set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}/RakDotNet)

add_library(${PROJECT_NAME} SHARED ${RAKDOTNET_SOURCES})

target_link_libraries(${PROJECT_NAME} ${LIBS} m)
