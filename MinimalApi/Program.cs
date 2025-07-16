using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinimalApi;
using MinimalApi.Data;
using MinimalApi.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseInMemoryDatabase("TodoList"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStatusCodePages();


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

app.MapPost("AddItem",async (AppDbContext db, TodoModel todoItem) =>
{
    
    db.Todo.Add(todoItem);
    await db.SaveChangesAsync();
    return Results.CreatedAtRoute<TodoModel>("GetItem",new {id = todoItem.Id}, todoItem);
}).WithTags("Task")
.Produces(201)
.Produces<ValidationProblem>(400)
;

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
.Produces(204);


app.Run();
