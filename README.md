# Ziedden.MySql für .Net
## Beschreibung
Mit diesen Parser ist es möglich Klassen direkt in/aus die MySql Datenbank zu speicher und zu laden.

## Benötigt
- MySql Connector/NET
- Newtonsoft.Json

# Infos
## Arbieten mit ID's
Ich rate jedem dazu mit den ID System zu arbeiten. Es ist deutlich Verwechslungssicherer.

## MySql Speicherung
Alles Daten die eingespeichert werden, werden im JSON vormat abgespeichert. Wenn die Datenbank noch anders verwendet wird z.B. mit PHP für einen Webiste muss dieses beachtet werden.

# HowTo
## Connection
Als erstes muss die Connection erstellt werden.
```csharp
  Ziedden.Mysql.Parser p = new Ziedden.Mysql.Parser("Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;");
```
oder 

```csharp
  Ziedden.Mysql.Parser p = new Ziedden.Mysql.Parser("myServerAddress","myUsername","myPassword","myDataBase");
```
## Attribute
Damit der Parser auch die Objecte erkennt müssen Attribute gesetzt werden z.B.:

```csharp
  public class User
  {
        [Mysql.MysqlData]
        public int ID;

        [Mysql.MysqlData]
        public string Username;

        [Mysql.MysqlData]
        public string Password;

        [Mysql.MysqlData]
        public string Email;
  }
```

Die ID wird immer angelegt. Diese wird als fortlaufende Nummer verwendet. 

## Insert
Der Insert Befehl ist um einen Eintrag zu tätigen. Ist keine Tabelle vorhanden so wird eine erstellt.
Der MySql Tabellen Name wird vom Klassennamen abgeleitet. Ist der Rückgabewert -1 ist ein Fehler unterlaufen.
```csharp
MySqlConnection connection = new MySqlConnection(ConnectionString);

//Init Klass
User newUser = new User();
newUser.Username = "Bsp";
newUser.Password = "Strong";
newUser.Email = "Bsp@example.com"

int inserID = connection.Insert(newUser);
Console.WriteLine("Eintrage wurde mit der ID {0} erstellt",inserID);

```

## Delete
Der Befehl Delete löscht einen Eintrag aus der Tabelle. Hier gibt es zwei möglichkeiten.

**NR 1**

In diesen Fall wird mit der ID gearbeitet.

```csharp
Ziedden.Mysql.Parser connection = new Ziedden.Mysql.Parser(ConnectionString);

int userID = 5;
connection.Delete(userID);
```

**NR 2**

In diesen Fall wird ohne ID gearbeitet.

```csharp
Ziedden.Mysql.Parser connection = new Ziedden.Mysql.Parser(ConnectionString);

string Username = "Bsp";
connection.Delte(typeof(User),new Ziedden.Mysql.Parser.Where[]  {new Ziedden.Mysql.Parser.Where("Username",Username)});
```

## FindWhere
Mit FindWhere kann man einen Eintrag mit dem passenden Daten suchen. Alles Einträge mit den passenden Daten werden als Array ausgegeben.

```csharp
Ziedden.Mysql.Parser connection = new Ziedden.Mysql.Parser(ConnectionString);

string Username = "Bsp";
string Email = "Bsp@example.com";
User[] foundUser = connection.FindWhere<User>(new Ziedden.Mysql.Parser.Where[]  {new Ziedden.Mysql.Parser.Where("Username",Username),new Ziedden.Mysql.Parser.Where("Email",Email)});
```

## FindByID
FindByID kann nur verwendet werden wenn die ID in der Klasse vorhanden ist (Siehe Bsp User).
```csharp
Ziedden.Mysql.Parser connection = new Ziedden.Mysql.Parser(ConnectionString);

int userID = 5;
User foundUser = connection.FindByID<User>(userID);
```

## Update
Mit dem Befehl Update kann man Einträge bearbieten. Hier gibt es auch wieder zwei Fälle. Die Funktion gibt einen bool zurück ob die Aktion erfolgreich war oder nicht.

**NR 1**

In diesen Fall wird mit der ID gearbeitet.

```csharp
Ziedden.Mysql.Parser connection = new Ziedden.Mysql.Parser(ConnectionString);

int userID = 5;
User foundUser = connection.FindByID(userID);

foundUser.Email = "newBsp@example.com";

connection.Update(foundUser);

```

**NR 2**

In diesen Fall wird ohne ID gearbeitet.

```csharp
Ziedden.Mysql.Parser connection = new Ziedden.Mysql.Parser(ConnectionString);

string Username = "Bsp";
User[] foundUsers = connection.FindWhere(new Ziedden.Mysql.Parser.Where[]  {new Ziedden.Mysql.Parser.Where("Username",Username)});

if(foundUsers.Count > 0)
{
  User[0].Email = "newBsp@example.com";
  connection.Update(User[0].Email,"Username",Username);
}

```

