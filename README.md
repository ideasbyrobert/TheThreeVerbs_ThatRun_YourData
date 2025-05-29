# The Three Verbs That Run Your Data

What if frontend and backend development could share the same DNA? This proof-of-concept explores a simple idea: every SQL query, at its core, is just Map, Filter and Reduce. Three operations. One mental model. Whether you're transforming data in React or querying a database, it's the same elegant pattern. All data manipulation might just be variations on three simple themes.

## Quick Start

### Prerequisites
- .NET 9 SDK
- Node.js 18+

### Run Server Examples
```bash
dotnet test
```

### Run Device Examples
```bash
cd TheaterSales.Device
npm install
npm test
```

## Project Structure

- **TheaterSales.Blueprint** - Reference implementations demonstrating Map-Filter-Reduce patterns in action
- **TheaterSales.Common** - Shared database fixtures and test data
- **TheaterSales.Device** - Implementation for user devices (browsers, mobile, IoT) with local resource processing
- **TheaterSales.Strategy** - The library providing multiple execution strategies (immediate, lazy, memoized, batched)
- **TheaterSales.Server** - Backend implementation with shared resource processing

## Core Insight

SQL operations are Map-Filter-Reduce in disguise:
- JOIN = Map (cartesian product) + Filter (on condition)  
- GROUP BY = Map (assign keys) + Reduce (aggregate)
- WHERE = Filter
- SELECT = Map

This means developers already know how to query data. They just need the right abstraction layer.

## Vision

Enable developers to write simple, functional code that works efficiently whether running on a user's device (unshared resources) or a backend server (shared resources). This exploration shows various execution strategies: lazy, memoized, batched. The future is simpler: write Map, Filter, Reduce. Behind the scenes, usage patterns determine optimal implementations. Performance without the performance tuning.