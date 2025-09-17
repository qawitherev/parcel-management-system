this file will talk about entity framework
what is it: like a programming interface to the database, so we dont directly talk to the database 
entity framework will do that 

<some things about entity framework>


Designing entity
Entity is just basically a table, e.g., User, Parcel, ResidentUnit... 
- when designing entity, we can mark the data to be nullable like below 
 public string? SafeWord { get; }
 public int? UserId { get; }

 and in the navigation property (if it is an foreign key)
 public User? User { get; }

 this is particularly useful when doing table metadata (the four things, createdBy/at, updatedBy/at)
 - where updatedBy/at is null when we first created the row 

 and in the entity configuration script, we would tag with .isRequired(false) to tell efCore that 
 this column is nullable 