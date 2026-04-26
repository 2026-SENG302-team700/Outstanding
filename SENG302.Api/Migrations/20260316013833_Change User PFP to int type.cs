using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SENG302.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserPFPtointtype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"ALTER TABLE ""Users"" 
          ALTER COLUMN ""ProfilePicture"" TYPE INTEGER 
          USING ""ProfilePicture""::integer"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"ALTER TABLE ""Users"" 
          ALTER COLUMN ""ProfilePicture"" TYPE TEXT 
          USING ""ProfilePicture""::text"
            );
        }
    }
}
