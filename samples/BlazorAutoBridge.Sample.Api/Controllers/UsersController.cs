using Microsoft.AspNetCore.Mvc;

namespace BlazorAutoBridge.Sample.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly List<User> _users = new()
    {
        new() { Id = 1, Username = "user1", Email = "user1@gmail.com" },
        new() { Id = 2, Username = "user2", Email = "user2@gmail.com" },
        new() { Id = 3, Username = "user3", Email = "user3@gmail.com" }
    };

    [HttpGet]
    public IActionResult GetAll([FromQuery] GetUsersRequest request)
    {
        return Ok(_users.Skip((request.Page - 1) * request.PerPage).Take(request.PerPage));
    }

    [HttpGet("{id}")]
    public IActionResult GetSingle(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public IActionResult Create([FromBody] User user)
    {
        if (_users.Any(u => u.Id == user.Id))
        {
            return Conflict("User with the same ID already exists.");
        }

        user.Id = _users.Max(u => u.Id) + 1;
        _users.Add(user);
        return CreatedAtAction(nameof(GetSingle), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] User updatedUser)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == id);
        if (existingUser == null)
        {
            return NotFound();
        }

        existingUser.Username = updatedUser.Username;
        existingUser.Email = updatedUser.Email;

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        _users.Remove(user);
        return NoContent();
    }
}


public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class GetUsersRequest
{
    public int Page { get; set; }
    public int PerPage { get; set; }
}