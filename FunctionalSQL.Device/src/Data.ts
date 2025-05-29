import Database from 'better-sqlite3'
import * as fs from 'fs'
import * as path from 'path'
import {Theater, Movie, Sale} from './Domain'

export class TheaterSalesContext
{
  private static TableNames = class
  {
    static readonly Theaters = "theaters";
    static readonly Movies = "movies";
    static readonly Sales = "sales";
  };

  private db: Database.Database

  constructor(connectionString: string)
  {
    this.db = new Database(connectionString)
  }

  get Theaters(): Theater[]
  {
    return this.loadAllFromTable<Theater>(TheaterSalesContext.TableNames.Theaters)
  }

  get Movies(): Movie[]
  {
    return this.loadAllFromTable<Movie>(TheaterSalesContext.TableNames.Movies)
  }

  get Sales(): Sale[]
  {
    return this.loadAllFromTable<Sale>(
      TheaterSalesContext.TableNames.Sales,
      this.mapSaleFromDatabase
    )
  }

  private loadAllFromTable<T>(tableName: string, mapper?: (row: any) => T): T[]
  {
    const statement = this.db.prepare(`SELECT * FROM ${tableName}`)
    const rows = statement.all()
    return mapper ? rows.map(mapper) : rows as T[]
  }

  private mapSaleFromDatabase(row: any): Sale
  {
    return {
      id: row.id,
      theaterId: row.theater_id,
      movieId: row.movie_id,
      saleDate: row.sale_date,
      amount: row.amount
    }
  }

  close(): void
  {
    this.db.close()
  }

}

export class DatabaseInitializer
{
  private static FileNames = class
  {
    static readonly Schema = "schema.sql";
    static readonly TestFixtures = "test_fixtures.sql";
  };

  private readonly _connectionString: string

  constructor(connectionString: string)
  {
    this._connectionString = connectionString
  }

  Initialize(): void
  {
    this.ExecuteSqlFile(DatabaseInitializer.FileNames.Schema)
  }

  SeedData(): void
  {
    this.ExecuteSqlFile(DatabaseInitializer.FileNames.TestFixtures)
  }

  private ExecuteSqlFile(fileName: string): void
  {
    const connection = new Database(this._connectionString)
    try
    {
      const sql = this.readSqlFile(fileName)
      connection.exec(sql)
    } finally
    {
      connection.close()
    }
  }

  private readSqlFile(fileName: string): string
  {
    const sqlPath = path.join(this.getBaseDirectory(), fileName)
    return fs.readFileSync(sqlPath, 'utf-8')
  }

  private getBaseDirectory(): string
  {
    const isInTestOrSourceDirectory = __dirname.includes('test') || __dirname.includes('src')
    return isInTestOrSourceDirectory ? path.join(__dirname, '..', 'dist') : __dirname
  }
}