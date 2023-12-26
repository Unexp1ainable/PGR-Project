#include "chessboard.h"
#include <cstring>

using namespace chessboard;
Configuration::Configuration() { std::memcpy(m_board, gameStart, sizeof(gameStart)); }

Configuration::Configuration(const Configuration& other) { std::memcpy(m_board, other.m_board, sizeof(m_board)); }

Configuration& Configuration::operator=(const Configuration& other)
{
    if (this != &other) {
        std::memcpy(m_board, other.m_board, sizeof(m_board));
    }
    return *this;
}
