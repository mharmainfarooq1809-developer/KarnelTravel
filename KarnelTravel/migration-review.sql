BEGIN TRANSACTION;
ALTER TABLE [TripInquiries] ADD [AdminNotes] nvarchar(2000) NULL;

ALTER TABLE [TripInquiries] ADD [OwnerEmail] nvarchar(256) NULL;

ALTER TABLE [TripInquiries] ADD [ProposedOffer] nvarchar(2000) NULL;

CREATE INDEX [IX_TripInquiries_OwnerEmail] ON [TripInquiries] ([OwnerEmail]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261108120401_AddCustomTripRequestOwnership', N'10.0.13');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [TripInquiries] ADD [ConvertedBookingId] int NULL;

ALTER TABLE [TripInquiries] ADD [IdempotencyKey] nvarchar(80) NULL;

ALTER TABLE [TripInquiries] ADD [OfferAcceptedAt] datetime2 NULL;

ALTER TABLE [TripInquiries] ADD [OfferIdempotencyKey] nvarchar(80) NULL;

ALTER TABLE [Bookings] ADD [IdempotencyKey] nvarchar(80) NULL;

ALTER TABLE [AIConversations] ADD [IsAnonymous] bit NOT NULL DEFAULT CAST(1 AS bit);

ALTER TABLE [AIConversations] ADD [OwnerEmail] nvarchar(256) NULL;

CREATE UNIQUE INDEX [IX_TripInquiries_IdempotencyKey] ON [TripInquiries] ([IdempotencyKey]) WHERE [IdempotencyKey] IS NOT NULL;

CREATE UNIQUE INDEX [IX_TripInquiries_OfferIdempotencyKey] ON [TripInquiries] ([OfferIdempotencyKey]) WHERE [OfferIdempotencyKey] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Bookings_IdempotencyKey] ON [Bookings] ([IdempotencyKey]) WHERE [IdempotencyKey] IS NOT NULL;

CREATE INDEX [IX_AIConversations_OwnerEmail] ON [AIConversations] ([OwnerEmail]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261108130131_AddConversationOwnershipAndSubmissionIdempotency', N'10.0.13');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [AIConversations] ADD [AnonymousOwnerKey] nvarchar(128) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261108132128_AddAnonymousAiOwnerBinding', N'10.0.13');

COMMIT;
GO

