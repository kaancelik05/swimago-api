using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swimago.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogCommentsForCustomerApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Table already exists in Supabase. Skipping manual creation block to bypass 42P07 error.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
