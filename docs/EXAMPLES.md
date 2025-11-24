# RazorHelpers Examples

Real-world examples demonstrating common use cases and scenarios.

## Table of Contents

- [E-Commerce Examples](#e-commerce-examples)
- [Blog and CMS Examples](#blog-and-cms-examples)
- [Dashboard and Admin Examples](#dashboard-and-admin-examples)
- [API Documentation Examples](#api-documentation-examples)
- [Email Templates](#email-templates)
- [Report Generation](#report-generation)

## E-Commerce Examples

### Complete Product Catalog

```csharp
// Models
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool InStock { get; set; }
}

// Templates
public static class ProductTemplates
{
    public static RenderFragment<Product> Card => product => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "product-card");
        builder.AddAttribute(seq++, "data-product-id", product.Id);

        // Product image
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "product-image");
        builder.OpenElement(seq++, "img");
        builder.AddAttribute(seq++, "src", product.ImageUrl);
        builder.AddAttribute(seq++, "alt", product.Name);
        builder.CloseElement();
        builder.CloseElement();

        // Product info
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "product-info");

        builder.OpenElement(seq++, "h3");
        builder.AddContent(seq++, product.Name);
        builder.CloseElement();

        builder.OpenElement(seq++, "p");
        builder.AddAttribute(seq++, "class", "category");
        builder.AddContent(seq++, product.Category);
        builder.CloseElement();

        // Rating
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "rating");
        for (int i = 1; i <= 5; i++)
        {
            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", i <= product.Rating ? "star filled" : "star");
            builder.AddContent(seq++, "★");
            builder.CloseElement();
        }
        builder.OpenElement(seq++, "span");
        builder.AddAttribute(seq++, "class", "review-count");
        builder.AddContent(seq++, $"({product.ReviewCount})");
        builder.CloseElement();
        builder.CloseElement();

        // Price
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "price");
        builder.AddContent(seq++, $"${product.Price:F2}");
        builder.CloseElement();

        // Stock status
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", product.InStock ? "in-stock" : "out-of-stock");
        builder.AddContent(seq++, product.InStock ? "In Stock" : "Out of Stock");
        builder.CloseElement();

        // Add to cart button
        if (product.InStock)
        {
            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "class", "btn-add-to-cart");
            builder.AddAttribute(seq++, "onclick", $"addToCart({product.Id})");
            builder.AddContent(seq++, "Add to Cart");
            builder.CloseElement();
        }

        builder.CloseElement();
        builder.CloseElement();
    };

    public static RenderFragment<List<Product>> Catalog => products => builder =>
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "product-catalog");

        // Group by category
        var groupedProducts = products.GroupBy(p => p.Category);

        foreach (var group in groupedProducts)
        {
            builder.OpenElement(seq++, "section");
            builder.AddAttribute(seq++, "class", "category-section");

            builder.OpenElement(seq++, "h2");
            builder.AddContent(seq++, group.Key);
            builder.CloseElement();

            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "product-grid");

            foreach (var product in group)
            {
                builder.AddContent(seq++, Card(product));
            }

            builder.CloseElement();
            builder.CloseElement();
        }

        builder.CloseElement();
    };
}

// Endpoints
app.MapGet("/products", async (ProductService productService) =>
{
    var products = await productService.GetAllProductsAsync();
    return RazorResults.Razor(ProductTemplates.Catalog, products);
});

app.MapGet("/product/{id:int}", async (int id, ProductService productService) =>
{
    var product = await productService.GetProductAsync(id);
    if (product == null)
        return Results.NotFound();

    return RazorResults.Razor(ProductTemplates.Card, product);
});
```

### Shopping Cart

```csharp
public class CartItem
{
    public Product Product { get; set; } = new();
    public int Quantity { get; set; }
    public decimal Subtotal => Product.Price * Quantity;
}

public class ShoppingCart
{
    public List<CartItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Subtotal);
    public int ItemCount => Items.Sum(i => i.Quantity);
}

RenderFragment<ShoppingCart> cartTemplate = cart => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "shopping-cart");

    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, $"Shopping Cart ({cart.ItemCount} items)");
    builder.CloseElement();

    if (cart.Items.Count == 0)
    {
        builder.OpenElement(seq++, "p");
        builder.AddAttribute(seq++, "class", "empty-cart");
        builder.AddContent(seq++, "Your cart is empty");
        builder.CloseElement();
    }
    else
    {
        builder.OpenElement(seq++, "table");
        builder.AddAttribute(seq++, "class", "cart-table");

        // Header
        builder.OpenElement(seq++, "thead");
        builder.OpenElement(seq++, "tr");
        builder.OpenElement(seq++, "th");
        builder.AddContent(seq++, "Product");
        builder.CloseElement();
        builder.OpenElement(seq++, "th");
        builder.AddContent(seq++, "Price");
        builder.CloseElement();
        builder.OpenElement(seq++, "th");
        builder.AddContent(seq++, "Quantity");
        builder.CloseElement();
        builder.OpenElement(seq++, "th");
        builder.AddContent(seq++, "Subtotal");
        builder.CloseElement();
        builder.CloseElement();
        builder.CloseElement();

        // Body
        builder.OpenElement(seq++, "tbody");
        foreach (var item in cart.Items)
        {
            builder.OpenElement(seq++, "tr");

            builder.OpenElement(seq++, "td");
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "product-info");
            builder.OpenElement(seq++, "img");
            builder.AddAttribute(seq++, "src", item.Product.ImageUrl);
            builder.AddAttribute(seq++, "width", "50");
            builder.CloseElement();
            builder.OpenElement(seq++, "span");
            builder.AddContent(seq++, item.Product.Name);
            builder.CloseElement();
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(seq++, "td");
            builder.AddContent(seq++, $"${item.Product.Price:F2}");
            builder.CloseElement();

            builder.OpenElement(seq++, "td");
            builder.AddContent(seq++, item.Quantity.ToString());
            builder.CloseElement();

            builder.OpenElement(seq++, "td");
            builder.AddContent(seq++, $"${item.Subtotal:F2}");
            builder.CloseElement();

            builder.CloseElement();
        }
        builder.CloseElement();

        // Footer with total
        builder.OpenElement(seq++, "tfoot");
        builder.OpenElement(seq++, "tr");
        builder.OpenElement(seq++, "td");
        builder.AddAttribute(seq++, "colspan", "3");
        builder.AddAttribute(seq++, "class", "text-right");
        builder.AddContent(seq++, "Total:");
        builder.CloseElement();
        builder.OpenElement(seq++, "td");
        builder.AddAttribute(seq++, "class", "cart-total");
        builder.AddContent(seq++, $"${cart.Total:F2}");
        builder.CloseElement();
        builder.CloseElement();
        builder.CloseElement();

        builder.CloseElement();

        // Checkout button
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "cart-actions");
        builder.OpenElement(seq++, "button");
        builder.AddAttribute(seq++, "class", "btn btn-primary btn-checkout");
        builder.AddAttribute(seq++, "onclick", "proceedToCheckout()");
        builder.AddContent(seq++, "Proceed to Checkout");
        builder.CloseElement();
        builder.CloseElement();
    }

    builder.CloseElement();
};

app.MapGet("/cart", (HttpContext context, CartService cartService) =>
{
    var cart = cartService.GetCart(context.Session.Id);
    return RazorResults.Razor(cartTemplate, cart);
});
```

## Blog and CMS Examples

### Blog Post with Comments

```csharp
public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
    public int ViewCount { get; set; }
}

public class Comment
{
    public string Author { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PostedDate { get; set; }
    public int Likes { get; set; }
}

RenderFragment<Comment> commentTemplate = comment => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "comment");

    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "comment-header");
    builder.OpenElement(seq++, "strong");
    builder.AddContent(seq++, comment.Author);
    builder.CloseElement();
    builder.OpenElement(seq++, "span");
    builder.AddAttribute(seq++, "class", "comment-date");
    builder.AddContent(seq++, comment.PostedDate.ToString("MMM dd, yyyy"));
    builder.CloseElement();
    builder.CloseElement();

    builder.OpenElement(seq++, "p");
    builder.AddAttribute(seq++, "class", "comment-content");
    builder.AddContent(seq++, comment.Content);
    builder.CloseElement();

    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "comment-actions");
    builder.OpenElement(seq++, "button");
    builder.AddAttribute(seq++, "class", "btn-like");
    builder.AddContent(seq++, $"👍 {comment.Likes}");
    builder.CloseElement();
    builder.CloseElement();

    builder.CloseElement();
};

RenderFragment<BlogPost> blogPostTemplate = post => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "article");
    builder.AddAttribute(seq++, "class", "blog-post");

    // Header
    builder.OpenElement(seq++, "header");
    builder.OpenElement(seq++, "h1");
    builder.AddContent(seq++, post.Title);
    builder.CloseElement();

    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "post-meta");
    builder.AddContent(seq++, $"By {post.Author} | {post.PublishedDate:MMMM dd, yyyy} | {post.ViewCount} views");
    builder.CloseElement();

    // Tags
    if (post.Tags.Any())
    {
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "post-tags");
        foreach (var tag in post.Tags)
        {
            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "tag");
            builder.AddContent(seq++, $"#{tag}");
            builder.CloseElement();
        }
        builder.CloseElement();
    }

    builder.CloseElement();

    // Content
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "post-content");
    builder.AddMarkupContent(seq++, post.Content);
    builder.CloseElement();

    // Comments section
    builder.OpenElement(seq++, "section");
    builder.AddAttribute(seq++, "class", "comments-section");

    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, $"Comments ({post.Comments.Count})");
    builder.CloseElement();

    if (post.Comments.Any())
    {
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "comments");
        foreach (var comment in post.Comments)
        {
            builder.AddContent(seq++, commentTemplate(comment));
        }
        builder.CloseElement();
    }
    else
    {
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, "No comments yet. Be the first to comment!");
        builder.CloseElement();
    }

    builder.CloseElement();

    builder.CloseElement();
};

app.MapGet("/blog/{slug}", async (string slug, BlogService blogService) =>
{
    var post = await blogService.GetPostBySlugAsync(slug);
    if (post == null)
        return Results.NotFound();

    await blogService.IncrementViewCountAsync(post.Id);
    return RazorResults.Razor(blogPostTemplate, post);
});
```

## Dashboard and Admin Examples

### Analytics Dashboard

```csharp
public class DashboardData
{
    public List<MetricCard> Metrics { get; set; } = new();
    public ChartData RevenueChart { get; set; } = new();
    public List<RecentActivity> Activities { get; set; } = new();
    public List<TopProduct> TopProducts { get; set; } = new();
}

public class MetricCard
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public string Icon { get; set; } = string.Empty;
}

RenderFragment<MetricCard> metricCardTemplate = metric => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "metric-card");

    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "metric-icon");
    builder.AddContent(seq++, metric.Icon);
    builder.CloseElement();

    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "metric-content");

    builder.OpenElement(seq++, "h3");
    builder.AddAttribute(seq++, "class", "metric-title");
    builder.AddContent(seq++, metric.Title);
    builder.CloseElement();

    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "metric-value");
    builder.AddContent(seq++, metric.Value);
    builder.CloseElement();

    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", metric.IsPositive ? "metric-change positive" : "metric-change negative");
    builder.AddContent(seq++, metric.IsPositive ? "▲ " : "▼ ");
    builder.AddContent(seq++, metric.Change);
    builder.CloseElement();

    builder.CloseElement();

    builder.CloseElement();
};

RenderFragment<DashboardData> dashboardTemplate = data => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "dashboard");

    builder.OpenElement(seq++, "h1");
    builder.AddContent(seq++, "Analytics Dashboard");
    builder.CloseElement();

    // Metrics row
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "metrics-grid");
    foreach (var metric in data.Metrics)
    {
        builder.AddContent(seq++, metricCardTemplate(metric));
    }
    builder.CloseElement();

    // Charts row
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "charts-row");
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "chart-container");
    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, "Revenue Trend");
    builder.CloseElement();
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "id", "revenue-chart");
    builder.AddAttribute(seq++, "data-chart", System.Text.Json.JsonSerializer.Serialize(data.RevenueChart));
    builder.CloseElement();
    builder.CloseElement();
    builder.CloseElement();

    // Two column layout
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "dashboard-columns");

    // Recent activity
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "activity-panel");
    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, "Recent Activity");
    builder.CloseElement();
    builder.OpenElement(seq++, "ul");
    builder.AddAttribute(seq++, "class", "activity-list");
    foreach (var activity in data.Activities)
    {
        builder.OpenElement(seq++, "li");
        builder.AddContent(seq++, activity.Description);
        builder.OpenElement(seq++, "small");
        builder.AddContent(seq++, activity.Timestamp.ToString("HH:mm"));
        builder.CloseElement();
        builder.CloseElement();
    }
    builder.CloseElement();
    builder.CloseElement();

    // Top products
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "top-products-panel");
    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, "Top Products");
    builder.CloseElement();
    builder.OpenElement(seq++, "ol");
    foreach (var product in data.TopProducts)
    {
        builder.OpenElement(seq++, "li");
        builder.AddContent(seq++, $"{product.Name} - {product.Sales} sales");
        builder.CloseElement();
    }
    builder.CloseElement();
    builder.CloseElement();

    builder.CloseElement(); // dashboard-columns

    builder.CloseElement(); // dashboard
};

app.MapGet("/dashboard", async (AnalyticsService analytics) =>
{
    var data = await analytics.GetDashboardDataAsync();
    return RazorResults.Razor(dashboardTemplate, data);
});
```

## Email Templates

### Order Confirmation Email

```csharp
public class OrderEmailData
{
    public int OrderNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public Address ShippingAddress { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Shipping { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string TrackingUrl { get; set; } = string.Empty;
}

RenderFragment<OrderEmailData> orderConfirmationEmail = order => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "html");
    builder.OpenElement(seq++, "body");
    builder.AddAttribute(seq++, "style", "font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;");

    // Header
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "style", "background: #4CAF50; color: white; padding: 20px; text-align: center;");
    builder.OpenElement(seq++, "h1");
    builder.AddContent(seq++, "Order Confirmation");
    builder.CloseElement();
    builder.CloseElement();

    // Greeting
    builder.OpenElement(seq++, "p");
    builder.AddAttribute(seq++, "style", "margin: 20px 0;");
    builder.AddContent(seq++, $"Hi {order.CustomerName},");
    builder.CloseElement();

    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, $"Thank you for your order! Your order #{order.OrderNumber} has been confirmed and will be shipped soon.");
    builder.CloseElement();

    // Order details
    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, "Order Details");
    builder.CloseElement();

    builder.OpenElement(seq++, "table");
    builder.AddAttribute(seq++, "style", "width: 100%; border-collapse: collapse;");

    // Items
    foreach (var item in order.Items)
    {
        builder.OpenElement(seq++, "tr");
        builder.AddAttribute(seq++, "style", "border-bottom: 1px solid #ddd;");

        builder.OpenElement(seq++, "td");
        builder.AddAttribute(seq++, "style", "padding: 10px;");
        builder.AddContent(seq++, $"{item.ProductName} x {item.Quantity}");
        builder.CloseElement();

        builder.OpenElement(seq++, "td");
        builder.AddAttribute(seq++, "style", "padding: 10px; text-align: right;");
        builder.AddContent(seq++, $"${item.Price * item.Quantity:F2}");
        builder.CloseElement();

        builder.CloseElement();
    }

    // Totals
    builder.OpenElement(seq++, "tr");
    builder.OpenElement(seq++, "td");
    builder.AddAttribute(seq++, "style", "padding: 10px;");
    builder.AddContent(seq++, "Subtotal");
    builder.CloseElement();
    builder.OpenElement(seq++, "td");
    builder.AddAttribute(seq++, "style", "padding: 10px; text-align: right;");
    builder.AddContent(seq++, $"${order.Subtotal:F2}");
    builder.CloseElement();
    builder.CloseElement();

    builder.OpenElement(seq++, "tr");
    builder.OpenElement(seq++, "td");
    builder.AddAttribute(seq++, "style", "padding: 10px;");
    builder.AddContent(seq++, "Shipping");
    builder.CloseElement();
    builder.OpenElement(seq++, "td");
    builder.AddAttribute(seq++, "style", "padding: 10px; text-align: right;");
    builder.AddContent(seq++, $"${order.Shipping:F2}");
    builder.CloseElement();
    builder.CloseElement();

    builder.OpenElement(seq++, "tr");
    builder.OpenElement(seq++, "td");
    builder.AddAttribute(seq++, "style", "padding: 10px;");
    builder.AddContent(seq++, "Tax");
    builder.CloseElement();
    builder.OpenElement(seq++, "td");
    builder.AddAttribute(seq++, "style", "padding: 10px; text-align: right;");
    builder.AddContent(seq++, $"${order.Tax:F2}");
    builder.CloseElement();
    builder.CloseElement();

    builder.OpenElement(seq++, "tr");
    builder.AddAttribute(seq++, "style", "font-weight: bold; font-size: 1.2em;");
    builder.OpenElement(seq++, "td");
    builder.AddAttribute(seq++, "style", "padding: 10px;");
    builder.AddContent(seq++, "Total");
    builder.CloseElement();
    builder.OpenElement(seq++, "td");
    builder.AddAttribute(seq++, "style", "padding: 10px; text-align: right;");
    builder.AddContent(seq++, $"${order.Total:F2}");
    builder.CloseElement();
    builder.CloseElement();

    builder.CloseElement(); // table

    // Shipping address
    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, "Shipping Address");
    builder.CloseElement();

    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, order.ShippingAddress.Street);
    builder.OpenElement(seq++, "br");
    builder.CloseElement();
    builder.AddContent(seq++, $"{order.ShippingAddress.City}, {order.ShippingAddress.State} {order.ShippingAddress.ZipCode}");
    builder.CloseElement();

    // Track order button
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "style", "text-align: center; margin: 30px 0;");
    builder.OpenElement(seq++, "a");
    builder.AddAttribute(seq++, "href", order.TrackingUrl);
    builder.AddAttribute(seq++, "style", "background: #4CAF50; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;");
    builder.AddContent(seq++, "Track Your Order");
    builder.CloseElement();
    builder.CloseElement();

    // Footer
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "style", "margin-top: 40px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 0.9em;");
    builder.AddContent(seq++, "If you have any questions, please contact our support team.");
    builder.CloseElement();

    builder.CloseElement(); // body
    builder.CloseElement(); // html
};

app.MapPost("/orders/{id}/send-confirmation", async (
    int id,
    IServiceProvider services,
    OrderService orderService,
    EmailService emailService) =>
{
    var order = await orderService.GetOrderAsync(id);
    if (order == null)
        return Results.NotFound();

    // Render email template
    var emailHtml = await orderConfirmationEmail.RenderAsync(order, services);

    // Send email
    await emailService.SendAsync(
        to: order.CustomerEmail,
        subject: $"Order Confirmation #{order.OrderNumber}",
        htmlBody: emailHtml
    );

    return Results.Ok();
});
```

## Report Generation

### PDF-Ready Sales Report

```csharp
public class SalesReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<SalesByCategory> CategoryBreakdown { get; set; } = new();
    public List<DailySales> DailySales { get; set; } = new();
}

RenderFragment<SalesReport> salesReportTemplate = report => builder =>
{
    var seq = 0;
    builder.OpenElement(seq++, "html");
    builder.OpenElement(seq++, "head");
    builder.OpenElement(seq++, "style");
    builder.AddContent(seq++, @"
        body { font-family: Arial, sans-serif; margin: 40px; }
        h1 { color: #333; border-bottom: 3px solid #4CAF50; padding-bottom: 10px; }
        .summary { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin: 30px 0; }
        .summary-card { background: #f5f5f5; padding: 20px; border-radius: 5px; }
        .summary-value { font-size: 2em; font-weight: bold; color: #4CAF50; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background: #4CAF50; color: white; }
        @media print { .no-print { display: none; } }
    ");
    builder.CloseElement();
    builder.CloseElement();

    builder.OpenElement(seq++, "body");

    // Header
    builder.OpenElement(seq++, "h1");
    builder.AddContent(seq++, "Sales Report");
    builder.CloseElement();

    builder.OpenElement(seq++, "p");
    builder.AddContent(seq++, $"Period: {report.StartDate:MM/dd/yyyy} - {report.EndDate:MM/dd/yyyy}");
    builder.CloseElement();

    // Summary cards
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "summary");

    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "summary-card");
    builder.OpenElement(seq++, "div");
    builder.AddContent(seq++, "Total Revenue");
    builder.CloseElement();
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "summary-value");
    builder.AddContent(seq++, $"${report.TotalRevenue:N2}");
    builder.CloseElement();
    builder.CloseElement();

    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "summary-card");
    builder.OpenElement(seq++, "div");
    builder.AddContent(seq++, "Total Orders");
    builder.CloseElement();
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "summary-value");
    builder.AddContent(seq++, report.TotalOrders.ToString("N0"));
    builder.CloseElement();
    builder.CloseElement();

    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "summary-card");
    builder.OpenElement(seq++, "div");
    builder.AddContent(seq++, "Average Order Value");
    builder.CloseElement();
    builder.OpenElement(seq++, "div");
    builder.AddAttribute(seq++, "class", "summary-value");
    builder.AddContent(seq++, $"${report.AverageOrderValue:N2}");
    builder.CloseElement();
    builder.CloseElement();

    builder.CloseElement(); // summary

    // Sales by category
    builder.OpenElement(seq++, "h2");
    builder.AddContent(seq++, "Sales by Category");
    builder.CloseElement();

    builder.OpenElement(seq++, "table");
    builder.OpenElement(seq++, "thead");
    builder.OpenElement(seq++, "tr");
    builder.OpenElement(seq++, "th");
    builder.AddContent(seq++, "Category");
    builder.CloseElement();
    builder.OpenElement(seq++, "th");
    builder.AddContent(seq++, "Revenue");
    builder.CloseElement();
    builder.OpenElement(seq++, "th");
    builder.AddContent(seq++, "Orders");
    builder.CloseElement();
    builder.OpenElement(seq++, "th");
    builder.AddContent(seq++, "Avg Value");
    builder.CloseElement();
    builder.CloseElement();
    builder.CloseElement();

    builder.OpenElement(seq++, "tbody");
    foreach (var category in report.CategoryBreakdown)
    {
        builder.OpenElement(seq++, "tr");
        builder.OpenElement(seq++, "td");
        builder.AddContent(seq++, category.CategoryName);
        builder.CloseElement();
        builder.OpenElement(seq++, "td");
        builder.AddContent(seq++, $"${category.Revenue:N2}");
        builder.CloseElement();
        builder.OpenElement(seq++, "td");
        builder.AddContent(seq++, category.OrderCount.ToString("N0"));
        builder.CloseElement();
        builder.OpenElement(seq++, "td");
        builder.AddContent(seq++, $"${category.AverageOrderValue:N2}");
        builder.CloseElement();
        builder.CloseElement();
    }
    builder.CloseElement();
    builder.CloseElement();

    // Print button
    builder.OpenElement(seq++, "button");
    builder.AddAttribute(seq++, "class", "no-print");
    builder.AddAttribute(seq++, "onclick", "window.print()");
    builder.AddAttribute(seq++, "style", "margin: 20px 0; padding: 10px 20px; background: #4CAF50; color: white; border: none; border-radius: 5px; cursor: pointer;");
    builder.AddContent(seq++, "Print Report");
    builder.CloseElement();

    builder.CloseElement(); // body
    builder.CloseElement(); // html
};

app.MapGet("/reports/sales", async (
    DateTime? startDate,
    DateTime? endDate,
    ReportService reportService) =>
{
    var report = await reportService.GenerateSalesReportAsync(
        startDate ?? DateTime.Now.AddMonths(-1),
        endDate ?? DateTime.Now
    );

    return RazorResults.Razor(salesReportTemplate, report);
});

// Generate PDF endpoint
app.MapGet("/reports/sales/pdf", async (
    DateTime? startDate,
    DateTime? endDate,
    IServiceProvider services,
    ReportService reportService,
    PdfService pdfService) =>
{
    var report = await reportService.GenerateSalesReportAsync(
        startDate ?? DateTime.Now.AddMonths(-1),
        endDate ?? DateTime.Now
    );

    // Render to HTML
    var html = await salesReportTemplate.RenderAsync(report, services);

    // Convert to PDF
    var pdfBytes = await pdfService.ConvertHtmlToPdfAsync(html);

    return Results.File(pdfBytes, "application/pdf", "sales-report.pdf");
});
```

---

## See Also

- [Usage Guide](USAGE.md)
- [API Reference](API.md)
- [Patterns and Best Practices](PATTERNS.md)
- [Troubleshooting](TROUBLESHOOTING.md)
