﻿namespace PrototypeGPT.Application.Providers;

/// <summary>
/// 
/// </summary>
public interface IAuthProvider
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    string UserName();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    string[] Roles();
}
