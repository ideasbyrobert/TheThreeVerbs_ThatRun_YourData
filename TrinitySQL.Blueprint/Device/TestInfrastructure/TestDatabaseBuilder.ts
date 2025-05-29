import {DatabaseInitializer, TheaterSalesContext} from '@device/Data'
import {TestDatabase} from './TestDatabase'
import * as path from 'path'
import {randomUUID} from 'crypto'

export class TestDatabaseBuilder
{
  build(): TestDatabase
  {
    const dbPath = this.generateDatabasePath()
    const connectionString = dbPath
    
    this.initializeDatabase(connectionString)
    
    const context = new TheaterSalesContext(connectionString)
    
    return new TestDatabase(context, dbPath)
  }
  
  private generateDatabasePath(): string
  {
    const tempDir = require('os').tmpdir()
    const uniqueId = randomUUID()
    return path.join(tempDir, `theater_sales_test_${uniqueId}.db`)
  }
  
  private initializeDatabase(connectionString: string): void
  {
    const initializer = new DatabaseInitializer(connectionString)
    initializer.Initialize()
    initializer.SeedData()
  }
}