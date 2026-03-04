using System;

namespace Identity.Application.Interfaces;

public interface ITokenHasher
{
    string HashToken(string token);
}
