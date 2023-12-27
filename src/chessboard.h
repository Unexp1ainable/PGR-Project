#pragma once
#include <cstring>
#include <geGL/Generated/OpenGLTypes.h>
#include <glm/glm.hpp>


namespace chessboard {
    enum Figure {
        EMPTY,
        PAWN,
        KING,
        QUEEN,
        BISHOP,
        KNIGHT,
        ROOK
    };

    enum Color {
        NONE,
        WHITE,
        BLACK
    };

    const glm::int32_t gameStart[8*8*2] = {
        ROOK, WHITE, KNIGHT, WHITE, BISHOP, WHITE, KING, WHITE, QUEEN, WHITE, BISHOP, WHITE, KNIGHT, WHITE, ROOK, WHITE,
        PAWN, WHITE, PAWN, WHITE, PAWN, WHITE, PAWN, WHITE, PAWN, WHITE, PAWN, WHITE, PAWN, WHITE, PAWN, WHITE,
        EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE,
        EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE,
        EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE,
        EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE, EMPTY, NONE,
        PAWN, BLACK, PAWN, BLACK, PAWN, BLACK, PAWN, BLACK, PAWN, BLACK, PAWN, BLACK, PAWN, BLACK, PAWN, BLACK,
        ROOK, BLACK, KNIGHT, BLACK, BISHOP, BLACK, QUEEN, BLACK, KING, BLACK, BISHOP, BLACK, KNIGHT, BLACK, ROOK, BLACK
    };


    /**
     * @brief Represents a configuration of a chessboard.
     * 
     * This class stores the configuration of a chessboard as an array of integers.
     * Each even integer represents a piece on the chessboard and odd integer color of the previous piece.
     */
    class Configuration {
    public:
        Configuration();
        Configuration(const Configuration& other);

        const GLint* data() const { return &m_board[0]; }

        Configuration& operator=(const Configuration& other);

        bool operator==(Configuration const& other) const
        {
            return std::memcmp(m_board, other.m_board, sizeof(m_board)) == 0;
        }

    private:
        glm::int32_t m_board[8*8*2];
    };
}

