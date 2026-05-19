using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LUPIAN_Activity.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeStudentSessionDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Students" ALTER COLUMN "TotalSessions" SET DEFAULT 30;
                ALTER TABLE "Students" ALTER COLUMN "RemainingSessions" SET DEFAULT 30;
                ALTER TABLE "Students" ALTER COLUMN "RewardPoints" SET DEFAULT 0;

                UPDATE "Students"
                SET
                    "TotalSessions" = CASE WHEN "TotalSessions" = 0 THEN 30 ELSE "TotalSessions" END,
                    "RemainingSessions" = CASE WHEN "RemainingSessions" = 0 THEN 30 ELSE "RemainingSessions" END,
                    "RewardPoints" = CASE WHEN "RewardPoints" = 30 THEN 0 ELSE "RewardPoints" END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Students" ALTER COLUMN "TotalSessions" SET DEFAULT 0;
                ALTER TABLE "Students" ALTER COLUMN "RemainingSessions" SET DEFAULT 30;
                ALTER TABLE "Students" ALTER COLUMN "RewardPoints" SET DEFAULT 30;
                """);
        }
    }
}
