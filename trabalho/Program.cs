using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.AspNetCore;
using Products.Data;
using Products.Model;

class HelloWeb
{
    static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "TodoAPI";
            config.Title = "TodoAPI v1";
            config.Version = "v1";
        });

        builder.Services.AddDbContext<AppDbContext>();

        var origins = "_origins";

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: origins,
            policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        WebApplication app = builder.Build();
        app.UseCors(origins);


        app.MapPost("/createProduct", (AppDbContext context, [FromBody] Product product) =>
{
    var newProduct = new Product(Guid.NewGuid(), product.nameProduct, product.price, product.amount);
    context.products.Add(newProduct);
    context.SaveChanges();
    return Results.Ok(newProduct);
}).Produces<Product>();



        app.MapGet("/allProducts", (AppDbContext context) =>
        {
            var prods = context.products;
            return prods is not null ? Results.Ok(prods) : Results.NotFound();
        }).Produces<Product>();

        app.MapGet("/", () =>
        {
        }).Produces<Product>();

        app.MapGet("/idProducts", (AppDbContext context, Guid inputId) =>
        {
            var prodsId = context.products.Find(inputId);
            return prodsId is not null ? Results.Ok(prodsId) : Results.NotFound();
        }).Produces<Product>();

        app.MapGet("/nameProducts", (AppDbContext context, String inputName) =>
        {
            var prodName = context.products.Where(p => p.nameProduct == inputName);
            return prodName is not null ? Results.Ok(prodName) : Results.NotFound();
        }).Produces<Product>();

        app.MapPatch("/patch", (AppDbContext context, Guid id, [FromBody] string newName) =>
        {
            var prod = context.products.Find(id);
            if (prod == null)
            {
                return Results.NotFound();
            }

            context.Entry(prod).State = EntityState.Detached;

            var updatedProduct = new Product(id, newName, prod.price, prod.amount);

            context.products.Update(updatedProduct);
            context.SaveChanges();

            return Results.Ok(updatedProduct);
        }).Produces<Product>();

        app.MapPut("/putProduct", (AppDbContext context, Product product) =>
        {
            var prodUpdate = context.products.Find(product.id);
            if (prodUpdate == null)
            {
                return Results.NotFound();
            }
            var entry = context.Entry(prodUpdate).CurrentValues;
            entry.SetValues(product);
            context.SaveChanges();
            return Results.Ok(product);
        }).Produces<Product>();

        app.MapDelete("/deleteProduct", (AppDbContext context, Guid id) =>
{
        var prodDelete = context.products.Find(id);
        if (prodDelete == null)
        {
            return Results.NotFound();
        }
            context.products.Remove(prodDelete);
            context.SaveChanges();
            return Results.Ok(prodDelete);
        }).Produces<Product>();



        app.Run();
    }
}