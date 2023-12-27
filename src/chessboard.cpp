#include "chessboard.h"
#include <cstring>

using namespace chessboard;
Configuration::Configuration(const std::string& config) 
{ 
    if (config == "start") {
        std::memcpy(m_board, gameStart, sizeof(gameStart)); 
    } else if (config == "random1") {
        std::memcpy(m_board, gameConfig1, sizeof(gameStart)); 
    } else if (config == "random2") {
        std::memcpy(m_board, gameConfig2, sizeof(gameStart)); 
    } else {
        std::memcpy(m_board, gameStart, sizeof(gameStart)); 
    }
}

Configuration::Configuration(const Configuration& other) { std::memcpy(m_board, other.m_board, sizeof(m_board)); }

Configuration& Configuration::operator=(const Configuration& other)
{
    if (this != &other) {
        std::memcpy(m_board, other.m_board, sizeof(m_board));
    }
    return *this;
}
