using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinimalApi;
using MinimalApi.Data;
using MinimalApi.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = "a-string-secret-at-least-256-bits-long";
// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseInMemoryDatabase("TodoList"));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
     options =>
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseHttpsRedirection();
app.UseStatusCodePages();

app.MapPost("/login", (AppDbContext db,UserModel login) =>
{
    var user = db.UserModels.FirstOrDefault(u => u.UserName == login.UserName); 
    if (login.Password == user.Password)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, login.UserName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Ok(new { token = tokenString });
    }

    return Results.BadRequest("Password or username doesn't match");
});


app.MapGet("GetUsers", async (AppDbContext db) =>
{
    var users = await db.UserModels.ToListAsync();
    return Results.Ok(users);
}).WithTags("User")
.Produces<IEnumerable<UserModel>>(200);

app.MapGet("GetUser/{id:int}", async (AppDbContext db, int id) =>
{
    var user = db.UserModels.FirstOrDefault(u => u.Id == id);
    if(user != null)
    {
        return Results.Ok(user);
    }
    return Results.NotFound();
}).WithTags("User")
.Produces<UserModel>(200)
.Produces(404);

app.MapPost("AddUser", async (AppDbContext db, UserModel user, IValidator<UserModel> validator) =>
{
    var validationResult = await validator.ValidateAsync(user);
    if (validationResult.IsValid)
    {
        db.UserModels.Add(user);
        await db.SaveChangesAsync();
        return Results.Ok(user);

    }
    return Results.BadRequest(validationResult.Errors);
}).WithTags("User")
.Produces(400)
.Produces<UserModel>(200);


app.MapGet("GetItems", async (AppDbContext db) =>
{
    var tasks = await db.Todo.Include(t => t.Notes).ToListAsync();
    return Results.Ok(tasks);
}).WithName("GetItems")
.WithTags("Task")
.Produces<TodoModel>(200);

app.MapGet("GetItem/{id}", async (AppDbContext db, int id) =>
{
    var task = await db.Todo.Include(t => t.Notes).FirstOrDefaultAsync(u => u.Id == id);

    if (task == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(task);
}).WithName("GetItem").
WithTags("Task")
.Produces<TodoModel>(201)
.Produces(404);

app.MapPost("AddItem",async (AppDbContext db, TodoModel todoItem, IValidator<TodoModel> validator) =>
{
    var validationRules = await validator.ValidateAsync(todoItem);
    if (validationRules.IsValid)
    {
        db.Todo.Add(todoItem);
        await db.SaveChangesAsync();
        return Results.CreatedAtRoute<TodoModel>("GetItem", new { id = todoItem.Id }, todoItem);
    }
    else
    {
        return Results.BadRequest(validationRules.Errors);
    }
   
}).WithTags("Task")
.Produces(201)
.Produces<ValidationProblem>(400);

app.MapPut("UpdateItem/{id:int}", async (AppDbContext db, int id, TodoModel updatedTask) =>
{
    if(id != updatedTask.Id)
    {
        return Results.BadRequest();
    }
    var task = await db.Todo.FirstOrDefaultAsync(u => u.Id == id);
    if(task == null)
    {
        return Results.NotFound();
    }
    task.Title = updatedTask.Title;
    task.IsCompleted = updatedTask.IsCompleted;

    return Results.Ok(task);

}).WithTags("Task")
.Produces(200)
.Produces(404);

app.MapDelete("DeleteItem/{id:int}", async (AppDbContext db, int id) =>
{
    var task = await db.Todo.Include(t => t.Notes).FirstOrDefaultAsync(u => u.Id == id);
    if (task == null)
    {
        return Results.NotFound();
    }

    db.Todo.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithTags("Task")
.Produces(404)
.Produces(400)
.Produces(204);

app.MapGet("SeachItem/", async (AppDbContext db, string? searchTerm) =>
{
    if (string.IsNullOrEmpty(searchTerm))
    {
        return Results.BadRequest();
    }
    var items = await db.Todo.Where(u => u.Title.Contains(searchTerm)).ToListAsync();
    return Results.Ok(items);
}).WithTags("Task")
.Produces(404)
.Produces(200)
;

app.Run();
