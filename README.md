# Matching-Cards
🃏 Memory Card Match Game
A complete card-matching memory game built with Unity, featuring smooth animations, multiple board layouts, save/load functionality, and cross-platform support.
🎮 Game Features

🎯 Classic Memory Gameplay: Flip cards to find matching pairs
📐 Multiple Board Sizes: Configurable layouts (2x2, 3x3, 4x4, 5x6, 6x8, etc.)
🎨 Smooth Animations: Card flip animations with easing curves
⚡ Continuous Gameplay: Non-blocking card interactions - flip multiple cards without waiting
💾 Save/Load System: JSON-based game state persistence
🏆 Scoring System: Points, combo multipliers, and time bonuses
🔊 Sound Effects: Flip, match, mismatch, and game completion sounds
📱 Cross-Platform: Desktop (Windows/Mac/Linux) + Mobile (Android/iOS)
🎛️ Customizable Settings: Adjustable board size and card scaling

🛠️ Technical Implementation
Architecture

No Singletons: Clean dependency injection pattern
Observer Pattern: Event-driven system for loose coupling
State Management: Proper game and card state handling
Component-Based: Modular, maintainable code structure

Design Patterns Used

Observer Pattern: Event system (OnCardMatched, OnScoreChanged, etc.)
State Pattern: Game states (Playing, Paused, GameWon) and card states
Factory Pattern: Board and card creation
Command Pattern: Input handling and game actions
Object Pooling: Efficient audio source management

Performance Optimizations

Coroutine-based animations for smooth performance
Component caching to minimize GetComponent calls
Efficient memory management with object pooling
Minimal garbage collection during gameplay
Scalable architecture supporting any board size
