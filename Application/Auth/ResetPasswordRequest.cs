namespace MyWebApi.Application.Auth;

public sealed record ResetPasswordRequest(
    int UserId,
    string NewPassword
);
