# The Three Verbs That Run Your Data

What if frontend and backend development could share the same DNA? This proof-of-concept explores a simple idea: every SQL query, at its core, is just Map, Filter and Reduce. Three operations. One mental model. Whether you're transforming data in React or querying a database, it's the same elegant pattern. All data manipulation might just be variations on three simple themes.

## Quick Start

### Prerequisites
- .NET 9 SDK
- Node.js 18+

### Run Tests
```bash
# Run all tests
dotnet test

# Run Server tests only
dotnet test --filter "TestCategory!=Device"

# Run Device tests only
dotnet test --filter "TestCategory=Device"
```

## Project Structure

- **TrinitySQL.Blueprint** - Reference implementations demonstrating Map-Filter-Reduce patterns in action
- **TrinitySQL.Context** - Theater domain context providing the case study for exploration
- **TrinitySQL.Core** - The foundational Map-Filter-Reduce operations with multiple execution strategies
- **TrinitySQL.Device** - Implementation for user devices (browsers, mobile, IoT) with local resource processing
- **TrinitySQL.Server** - Backend implementation with shared resource processing

## Core Insight

SQL operations are Map-Filter-Reduce in disguise:
- JOIN = Map (cartesian product) + Filter (on condition)  
- GROUP BY = Map (assign keys) + Reduce (aggregate)
- WHERE = Filter
- SELECT = Map

This means developers already know how to query data. They just need the right abstraction layer.

## Vision

Enable developers to write simple, functional code that works efficiently whether running on a user's device (unshared resources) or a backend server (shared resources). TrinitySQL.Core provides the foundational operations with various execution strategies: lazy, memoized, batched. The future is simpler: write Map, Filter, Reduce. Behind the scenes, usage patterns determine optimal implementations. Performance without the performance tuning.

## Technical Architecture Guide

*Vision without architecture is hallucination. This guide grounds TrinitySQL in concrete patterns.*

### Navigation

- [Leaky Abstractions](#leaky-abstractions)
- [Optimization Strategies](#optimization-strategies)
- [Correctness Through Comparison](#correctness-through-comparison)
- [Bounded Contexts](#bounded-contexts)
- [Open Questions](#open-questions)

---

## Leaky Abstractions

Databases execute SQL, not Map-Filter-Reduce. ORMs try to bridge this gap but become black boxes that obscure performance and intent.

TrinitySQL investigates a different path: implement Map-Filter-Reduce directly. No translation layers. No query generation. Just pure functional transformations.

Currently, code execution strategies are explicitly chosen. Future exploration will focus on auto-tuning performance through usage patterns: no LazyMap, no BatchFilter, no MemoReduce. Just Map, Filter, Reduce.

## Optimization Strategies

RDBMS engines optimize SQL through decades of engineering: query planners, indexes, statistics, parallel execution. When implementing Map-Filter-Reduce directly, we lose these optimizations.

TrinitySQL explores strategies to bridge this performance gap. Each strategy represents a different approach to efficient data transformation without a query optimizer:

### Basic Strategy
*Location: `TrinitySQL.Core/MapFilterReduce/Strategies/Basic/`*

Direct, eager execution. Every transformation immediately processes all data.

**When to use**: Small datasets, simple transformations, when you need all results immediately.

### Lazy Strategy  
*Location: `TrinitySQL.Core/MapFilterReduce/Strategies/Lazy/`*

Deferred execution using iterators. Transformations compose without materializing intermediate results.

**When to use**: Large datasets, chained operations, when you might not consume all results.

### Memoized Strategy
*Location: `TrinitySQL.Core/MapFilterReduce/Strategies/Memoization/`*

Caches computation results. Repeated operations return cached values without recalculation.

**When to use**: Expensive computations, repeated queries, aggregations over slowly-changing data.

### Batch Strategy
*Location: `TrinitySQL.Core/MapFilterReduce/Strategies/Batch/`*

Processes data in configurable chunks. Balances memory usage with performance.

**When to use**: Very large datasets, memory-constrained environments, streaming scenarios.

### Composite Strategy
*Location: `TrinitySQL.Core/MapFilterReduce/Strategies/Composite/`*

Combines multiple transformations into a single pass. Optimizes by eliminating intermediate collections.

**When to use**: Complex transformation chains, performance-critical paths, reducing memory allocation.

## Correctness Through Comparison

Map-Filter-Reduce implementations prove their correctness through baseline comparison testing.

### The Pattern

1. **Baseline Query** (traditional ORM)  
   *Location: `TrinitySQL.Blueprint/Server/BoundedContexts/TheaterPerformance/Baseline/`*
   
   ```
   TheatersByDateQuery.cs - Standard database query using ORM
   ```

2. **Map-Filter-Reduce Implementation**  
   *Location: `TrinitySQL.Server/src/BoundedContexts/TheaterPerformance/Application/QueryHandlers/`*
   
   ```
   GetTheatersByDateQueryHandler.cs - Map-Filter-Reduce implementation
   ```

3. **Comparison Test**  
   *Location: `TrinitySQL.Blueprint/Server/BoundedContexts/TheaterPerformance/Application/QueryHandlers/`*
   
   ```
   GetTheatersByDateQueryHandlerTests.cs - Proves both produce identical results
   ```

Both implementations run against the same test data in `TrinitySQL.Context/test_fixtures.sql`. 

## Bounded Contexts

Each bounded context encapsulates related business logic:

```
TrinitySQL.Server/src/BoundedContexts/TheaterPerformance/
├── Domain/
│   ├── ValueObjects/       # Immutable domain concepts
│   │   ├── DateRange.cs
│   │   └── TheaterPerformanceResult.cs
│   └── Events/            # Domain events
│       └── TheaterPerformanceQueriedEvent.cs
└── Application/
    ├── Queries/           # Query definitions (implements IQuery<T>)
    │   └── GetTheatersByDateQuery.cs
    └── QueryHandlers/     # Query implementations
        └── GetTheatersByDateQueryHandler.cs
```

## Open Questions

Several fundamental questions drive this investigation forward.

**What are the performance boundaries?** Each strategy has theoretical limits. Memoization trades memory for speed. Lazy evaluation reduces memory at the cost of computation. Where exactly do these trade-offs break down?

**Should Map-Filter-Reduce strategies build their own indexes?** RDBMS engines use B-trees and hash tables for logarithmic lookups. Currently, the functional approach scans linearly. Would introducing index structures violate the simplicity of pure transformations, or do they become necessary at scale?

---

*As the exploration progresses, this README will include collaboration guidelines. For now, interested contributors can submit pull requests.*