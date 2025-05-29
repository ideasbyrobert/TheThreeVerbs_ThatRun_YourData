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

class SqlFileLocator
{
  private readonly markerFileName: string
  private readonly searchStrategies: SearchStrategy[]
  
  constructor(markerFileName: string, searchStrategies: SearchStrategy[])
  {
    this.markerFileName = markerFileName
    this.searchStrategies = searchStrategies
  }

  findSqlDirectory(): string
  {
    const searchPaths = this.collectSearchPaths()
    const foundPath = this.findFirstValidPath(searchPaths)
    
    if (!foundPath)
    {
      throw new SqlFilesNotFoundError(searchPaths)
    }
    
    return foundPath
  }

  private collectSearchPaths(): string[]
  {
    return this.searchStrategies.map(strategy => strategy.getPath())
  }

  private findFirstValidPath(paths: string[]): string | undefined
  {
    return paths.find(path => this.containsSqlFile(path))
  }

  private containsSqlFile(directory: string): boolean
  {
    const filePath = path.join(directory, this.markerFileName)
    return fs.existsSync(filePath)
  }
}

interface SearchStrategy
{
  getPath(): string
}

class CurrentDirectoryStrategy implements SearchStrategy
{
  getPath(): string
  {
    return __dirname
  }
}

class RelativePathStrategy implements SearchStrategy
{
  private readonly relativePath: string

  constructor(relativePath: string)
  {
    this.relativePath = relativePath
  }

  getPath(): string
  {
    return path.join(__dirname, ...this.relativePath.split('/'))
  }
}

class WorkingDirectoryStrategy implements SearchStrategy
{
  private readonly subPath: string

  constructor(subPath: string)
  {
    this.subPath = subPath
  }

  getPath(): string
  {
    return path.join(process.cwd(), this.subPath)
  }
}

class SqlFilesNotFoundError extends Error
{
  constructor(searchedPaths: string[])
  {
    super(`Could not find SQL files. Searched in: ${searchedPaths.join(', ')}`)
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
    const searchStrategies = [
      new CurrentDirectoryStrategy(),
      new RelativePathStrategy('../dist'),
      new RelativePathStrategy('../../../TrinitySQL.Context'),
      new WorkingDirectoryStrategy('TrinitySQL.Context')
    ]
    
    const sqlFileLocator = new SqlFileLocator(
      DatabaseInitializer.FileNames.Schema, 
      searchStrategies
    )
    
    return sqlFileLocator.findSqlDirectory()
  }
}