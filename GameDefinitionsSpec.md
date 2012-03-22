Game Definitions Spec
============

## Overview

The BBot Game Definitions (BBGD) library describes the core components of the Bejewelled Blitz game and provides functions to simulate gameplay.

## Scenarios

*Scenario 1*
Given an image of a game board, synthesize a representation into a useful form. This will use a set of known heuristics for each game piece, and make a best-guess effort to identify game pieces on the game board.

*Scenario 2*
Given a game board, find any possible moves. This will use an exhaustive search to determine any valid moves which generate a match for the current board. Also, find best possible move. This will make a best-guess effort to sort valid moves by likelihood to produce large chains or multiple matches.

*Scenario 3*
Given a game board and a valid move, determine subsequent game board state. This will utilise game play rules to simulate the effects of a move.

*Scenario 4* 
Given an image of a game board, improve known heuristics by manually identifying game pieces. This will use suggested matches to overwrite known heuristics for game pieces.

*Scenario 5*
Given a game board, create a graphical representation. This will generate an image based on the board.

## Nongoals

The BBGD library does not support the following:

 * interacting with the game
 * 

## Details, details, details

Game Screen Definition

Board Definition

Gem Definitions

Find Bitmap Worker

## Open Issues
