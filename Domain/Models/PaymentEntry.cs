public sealed record PaymentEntry(
    long PaymentId,
    string PaymentNo,
    DateOnly PaymentDate,
    string PartyType,
    string PartyName,
    string PaymentType,
    decimal Amount,
    string Status,
    long? VoucherId
);
