using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Single source of truth for all job definitions.
/// Server and client both read this — it is never modified at runtime.
///
/// Tier:       Free  = available to all players.
///             Vip   = requires VIP status to select.
///
/// HourlyWage: Real-world-inspired lower-end hourly rate in in-game dollars.
///             Salaried roles converted via: yearly / 2080 = hourly.
///             Crime / underground jobs are 0 — they earn through crime payouts.
///
/// SalaryAmount: HourlyWage * 3.  Assumes EconomySystem.SalaryInterval = 1200 s
///               (20 game minutes), giving 3 payouts per game-hour.
///               Adjust the multiplier in Salary() if the interval changes.
///
/// To add a job: append to the correct section below. That's it.
/// </summary>
public static class JobRegistry
{
    private static readonly Dictionary<string, JobDefinition> _jobs;

    // SalaryAmount = HourlyWage * 3  (3 payouts per game-hour at 20-min intervals).
    // Crime jobs always return 0.
    private static int Salary( int hourlyWage ) => hourlyWage == 0 ? 0 : Math.Max( 10, hourlyWage * 3 );

    static JobRegistry()
    {
        var list = new List<JobDefinition>
        {
            // ─────────────────────────────────────────────────────────────────
            // CATEGORY A — Civilian / Everyday Life
            // ─────────────────────────────────────────────────────────────────
            new() { Id = "citizen",            DisplayName = "Citizen",            Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 12,  SalaryAmount = Salary(12),  MaxSlots = -1,
                Description = "An ordinary resident of the city." },
            new() { Id = "unemployed",         DisplayName = "Unemployed",         Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 0,   SalaryAmount = 10,          MaxSlots = -1,
                Description = "No job, no income. Survive however you can." },
            new() { Id = "office_worker",      DisplayName = "Office Worker",      Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 20,  SalaryAmount = Salary(20),  MaxSlots = -1,
                Description = "White-collar desk job. Stable, unremarkable." },
            new() { Id = "clerk",              DisplayName = "Clerk",              Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 16,  SalaryAmount = Salary(16),  MaxSlots = -1,
                Description = "General store clerk." },
            new() { Id = "convenience_clerk",  DisplayName = "Convenience Clerk",  Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 16,  SalaryAmount = Salary(16),  MaxSlots = -1,
                Description = "Works the counter at a convenience store." },
            new() { Id = "cashier",            DisplayName = "Cashier",            Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 16,  SalaryAmount = Salary(16),  MaxSlots = -1,
                Description = "Runs a register. Eyes on everyone who walks in." },
            new() { Id = "grocer",             DisplayName = "Grocer",             Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 17,  SalaryAmount = Salary(17),  MaxSlots = -1,
                Description = "Stocks shelves and sells produce." },
            new() { Id = "deli_worker",        DisplayName = "Deli Worker",        Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 16,  SalaryAmount = Salary(16),  MaxSlots = -1,
                Description = "Slices meat and makes sandwiches." },
            new() { Id = "fast_food_worker",   DisplayName = "Fast Food Worker",   Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 16,  SalaryAmount = Salary(16),  MaxSlots = -1,
                Description = "Flips burgers and works the counter." },
            new() { Id = "fry_cook",           DisplayName = "Fry Cook",           Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 16,  SalaryAmount = Salary(16),  MaxSlots = -1,
                Description = "Deep-fryer specialist. Grease and heat." },
            new() { Id = "cook",               DisplayName = "Cook",               Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 18,  SalaryAmount = Salary(18),  MaxSlots = -1,
                Description = "Prepares meals in a restaurant kitchen." },
            new() { Id = "chef",               DisplayName = "Chef",               Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 24,  SalaryAmount = Salary(24),  MaxSlots = -1,
                Description = "Head of the kitchen. Commands the line." },
            new() { Id = "sous_chef",          DisplayName = "Sous Chef",          Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 21,  SalaryAmount = Salary(21),  MaxSlots = -1,
                Description = "Second-in-command of the kitchen." },
            new() { Id = "baker",              DisplayName = "Baker",              Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 17,  SalaryAmount = Salary(17),  MaxSlots = -1,
                Description = "Bakes bread, pastries, and cakes." },
            new() { Id = "butcher",            DisplayName = "Butcher",            Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 20,  SalaryAmount = Salary(20),  MaxSlots = -1,
                Description = "Cuts and preps meat for sale." },
            new() { Id = "barista",            DisplayName = "Barista",            Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 16,  SalaryAmount = Salary(16),  MaxSlots = -1,
                Description = "Makes coffee. Knows your order." },
            new() { Id = "bartender",          DisplayName = "Bartender",          Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 18,  SalaryAmount = Salary(18),  MaxSlots = -1,
                Description = "Pours drinks and keeps the peace at the bar." },
            new() { Id = "waiter",             DisplayName = "Waiter",             Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 17,  SalaryAmount = Salary(17),  MaxSlots = -1,
                Description = "Serves tables at a restaurant." },
            new() { Id = "waitress",           DisplayName = "Waitress",           Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 17,  SalaryAmount = Salary(17),  MaxSlots = -1,
                Description = "Serves tables at a restaurant." },
            new() { Id = "janitor",            DisplayName = "Janitor",            Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 18,  SalaryAmount = Salary(18),  MaxSlots = -1,
                Description = "Keeps buildings clean. Invisible to most." },
            new() { Id = "cleaner",            DisplayName = "Cleaner",            Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 17,  SalaryAmount = Salary(17),  MaxSlots = -1,
                Description = "Residential or commercial cleaning services." },
            new() { Id = "laundromat_worker",  DisplayName = "Laundromat Worker",  Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 16,  SalaryAmount = Salary(16),  MaxSlots = -1,
                Description = "Runs the laundromat. Sees a lot of dirty laundry." },
            new() { Id = "hotel_worker",       DisplayName = "Hotel Worker",       Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 18,  SalaryAmount = Salary(18),  MaxSlots = -1,
                Description = "Handles check-in, housekeeping, or front desk." },
            new() { Id = "motel_manager",      DisplayName = "Motel Manager",      Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 22,  SalaryAmount = Salary(22),  MaxSlots = -1,
                Description = "Runs a roadside motel. No questions asked." },
            new() { Id = "club_promoter",      DisplayName = "Club Promoter",      Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 20,  SalaryAmount = Salary(20),  MaxSlots = -1,
                Description = "Fills venues. Knows everyone." },
            new() { Id = "dj",                 DisplayName = "DJ",                 Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 22,  SalaryAmount = Salary(22),  MaxSlots = -1,
                Description = "Spins tracks and controls the room's energy." },
            new() { Id = "dancer",             DisplayName = "Dancer",             Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 16,  SalaryAmount = Salary(16),  MaxSlots = -1,
                Description = "Performs at clubs and events." },
            new() { Id = "adult_entertainer",  DisplayName = "Adult Entertainer",  Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 16,  SalaryAmount = Salary(16),  MaxSlots = -1,
                Description = "Works at adult venues. Legal work, legal pay." },
            new() { Id = "escort",             DisplayName = "Escort",             Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 16,  SalaryAmount = Salary(16),  MaxSlots = -1,
                Description = "Paid companionship. Strictly above board." },
            new() { Id = "street_worker",      DisplayName = "Street Worker",      Department = "Civilian",   Tier = JobTier.Free, HourlyWage = 16,  SalaryAmount = Salary(16),  MaxSlots = -1,
                Description = "Earns a living on the street." },

            // ─────────────────────────────────────────────────────────────────
            // CATEGORY B — Housing / Property / Commerce
            // ─────────────────────────────────────────────────────────────────
            new() { Id = "landlord",           DisplayName = "Landlord",           Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 30,  SalaryAmount = Salary(30),  MaxSlots = -1,
                Description = "Owns and rents out properties." },
            new() { Id = "property_manager",   DisplayName = "Property Manager",   Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 28,  SalaryAmount = Salary(28),  MaxSlots = -1,
                Description = "Manages properties on behalf of owners." },
            new() { Id = "real_estate_agent",  DisplayName = "Real Estate Agent",  Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 30,  SalaryAmount = Salary(30),  MaxSlots = -1,
                Description = "Buys and sells property. Commission-driven." },
            new() { Id = "pawn_broker",        DisplayName = "Pawn Broker",        Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 20,  SalaryAmount = Salary(20),  MaxSlots = -1,
                Description = "Buys and sells second-hand goods. No questions asked." },
            new() { Id = "used_car_dealer",    DisplayName = "Used Car Dealer",    Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 20,  SalaryAmount = Salary(20),  MaxSlots = -1,
                Description = "Flips pre-owned vehicles." },
            new() { Id = "dealership_salesman",DisplayName = "Dealership Salesman",Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 22,  SalaryAmount = Salary(22),  MaxSlots = -1,
                Description = "Sells new vehicles at a dealership." },
            new() { Id = "hardware_store_owner",DisplayName = "Hardware Store Owner",Department = "Commerce", Tier = JobTier.Free, HourlyWage = 25,  SalaryAmount = Salary(25),  MaxSlots = -1,
                Description = "Runs a hardware and tools shop." },
            new() { Id = "liquor_store_owner", DisplayName = "Liquor Store Owner", Department = "Commerce",   Tier = JobTier.Vip,  HourlyWage = 28,  SalaryAmount = Salary(28),  MaxSlots = 2,
                Description = "Owns and operates a licensed liquor store." },
            new() { Id = "bodega_owner",       DisplayName = "Bodega Owner",       Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 22,  SalaryAmount = Salary(22),  MaxSlots = -1,
                Description = "Runs a corner bodega. Community staple." },
            new() { Id = "restaurant_owner",   DisplayName = "Restaurant Owner",   Department = "Commerce",   Tier = JobTier.Vip,  HourlyWage = 38,  SalaryAmount = Salary(38),  MaxSlots = 2,
                Description = "Owns a restaurant. Hires staff, sets the menu." },
            new() { Id = "club_owner",         DisplayName = "Club Owner",         Department = "Commerce",   Tier = JobTier.Vip,  HourlyWage = 42,  SalaryAmount = Salary(42),  MaxSlots = 2,
                Description = "Owns a nightclub. Controls who gets in." },
            new() { Id = "storage_manager",    DisplayName = "Storage Manager",    Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 22,  SalaryAmount = Salary(22),  MaxSlots = -1,
                Description = "Manages a storage facility. Knows what's inside." },
            new() { Id = "warehouse_manager",  DisplayName = "Warehouse Manager",  Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 28,  SalaryAmount = Salary(28),  MaxSlots = -1,
                Description = "Oversees a large warehouse operation." },
            new() { Id = "banker",             DisplayName = "Banker",             Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 45,  SalaryAmount = Salary(45),  MaxSlots = 3,
                Description = "Manages accounts and transactions at the bank." },
            new() { Id = "loan_officer",       DisplayName = "Loan Officer",       Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 38,  SalaryAmount = Salary(38),  MaxSlots = 2,
                Description = "Approves or denies loan applications." },
            new() { Id = "accountant",         DisplayName = "Accountant",         Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 36,  SalaryAmount = Salary(36),  MaxSlots = -1,
                Description = "Handles finances for businesses and individuals." },
            new() { Id = "insurance_agent",    DisplayName = "Insurance Agent",    Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 28,  SalaryAmount = Salary(28),  MaxSlots = -1,
                Description = "Sells and manages insurance policies." },
            new() { Id = "lawyer",             DisplayName = "Lawyer",             Department = "Commerce",   Tier = JobTier.Vip,  HourlyWage = 65,  SalaryAmount = Salary(65),  MaxSlots = 4,
                Description = "Represents clients in legal matters." },
            new() { Id = "paralegal",          DisplayName = "Paralegal",          Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 24,  SalaryAmount = Salary(24),  MaxSlots = -1,
                Description = "Assists lawyers with research and filings." },
            new() { Id = "bail_bondsman",      DisplayName = "Bail Bondsman",      Department = "Commerce",   Tier = JobTier.Free, HourlyWage = 26,  SalaryAmount = Salary(26),  MaxSlots = -1,
                Description = "Posts bail for arrested players. Gets them out fast." },
            new() { Id = "bank_director",      DisplayName = "Bank Director",      Department = "Commerce",   Tier = JobTier.Vip,  HourlyWage = 58,  SalaryAmount = Salary(58),  MaxSlots = 1,
                Description = "Runs the entire bank. Controls the vault." },
            new() { Id = "corporate_executive",DisplayName = "Corporate Executive",Department = "Commerce",   Tier = JobTier.Vip,  HourlyWage = 65,  SalaryAmount = Salary(65),  MaxSlots = 2,
                Description = "C-suite leadership of a major corporation." },
            new() { Id = "luxury_dealer",      DisplayName = "Luxury Dealer",      Department = "Commerce",   Tier = JobTier.Vip,  HourlyWage = 50,  SalaryAmount = Salary(50),  MaxSlots = 2,
                Description = "Sells high-end goods, vehicles, and art." },

            // ─────────────────────────────────────────────────────────────────
            // CATEGORY C — Infrastructure / Trades / Utilities
            // ─────────────────────────────────────────────────────────────────
            new() { Id = "mechanic",           DisplayName = "Mechanic",           Department = "Trades",     Tier = JobTier.Free, HourlyWage = 24,  SalaryAmount = Salary(24),  MaxSlots = -1,
                Description = "Repairs and services vehicles." },
            new() { Id = "senior_mechanic",    DisplayName = "Senior Mechanic",    Department = "Trades",     Tier = JobTier.Free, HourlyWage = 30,  SalaryAmount = Salary(30),  MaxSlots = -1,
                Description = "Advanced vehicle tech. Can handle rare repairs." },
            new() { Id = "tow_operator",       DisplayName = "Tow Operator",       Department = "Trades",     Tier = JobTier.Free, HourlyWage = 22,  SalaryAmount = Salary(22),  MaxSlots = -1,
                Description = "Hauls disabled vehicles. Knows every parking lot." },
            new() { Id = "electrician",        DisplayName = "Electrician",        Department = "Trades",     Tier = JobTier.Free, HourlyWage = 30,  SalaryAmount = Salary(30),  MaxSlots = -1,
                Description = "Wires and maintains electrical systems." },
            new() { Id = "apprentice_electrician",DisplayName = "Apprentice Electrician",Department = "Trades",Tier = JobTier.Free,HourlyWage = 22,  SalaryAmount = Salary(22),  MaxSlots = -1,
                Description = "Learning the trade under a licensed electrician." },
            new() { Id = "plumber",            DisplayName = "Plumber",            Department = "Trades",     Tier = JobTier.Free, HourlyWage = 29,  SalaryAmount = Salary(29),  MaxSlots = -1,
                Description = "Installs and repairs pipes and water systems." },
            new() { Id = "pipefitter",         DisplayName = "Pipefitter",         Department = "Trades",     Tier = JobTier.Free, HourlyWage = 31,  SalaryAmount = Salary(31),  MaxSlots = -1,
                Description = "Works with high-pressure pipe systems." },
            new() { Id = "construction_worker",DisplayName = "Construction Worker",Department = "Trades",     Tier = JobTier.Free, HourlyWage = 23,  SalaryAmount = Salary(23),  MaxSlots = -1,
                Description = "Builds structures. Heavy lifting." },
            new() { Id = "foreman",            DisplayName = "Foreman",            Department = "Trades",     Tier = JobTier.Free, HourlyWage = 34,  SalaryAmount = Salary(34),  MaxSlots = -1,
                Description = "Supervises construction crews on-site." },
            new() { Id = "welder",             DisplayName = "Welder",             Department = "Trades",     Tier = JobTier.Free, HourlyWage = 25,  SalaryAmount = Salary(25),  MaxSlots = -1,
                Description = "Fuses metal. Works in construction and repair." },
            new() { Id = "hvac_technician",    DisplayName = "HVAC Technician",    Department = "Trades",     Tier = JobTier.Free, HourlyWage = 28,  SalaryAmount = Salary(28),  MaxSlots = -1,
                Description = "Installs and repairs heating and cooling systems." },
            new() { Id = "telecom_technician", DisplayName = "Telecom Technician", Department = "Trades",     Tier = JobTier.Free, HourlyWage = 27,  SalaryAmount = Salary(27),  MaxSlots = -1,
                Description = "Maintains phone and network infrastructure." },
            new() { Id = "cable_installer",    DisplayName = "Cable Installer",    Department = "Trades",     Tier = JobTier.Free, HourlyWage = 24,  SalaryAmount = Salary(24),  MaxSlots = -1,
                Description = "Runs cable lines to homes and businesses." },
            new() { Id = "utility_worker",     DisplayName = "Utility Worker",     Department = "Trades",     Tier = JobTier.Free, HourlyWage = 27,  SalaryAmount = Salary(27),  MaxSlots = -1,
                Description = "Maintains city power and water utilities." },
            new() { Id = "city_maintenance",   DisplayName = "City Maintenance Worker",Department = "Trades", Tier = JobTier.Free, HourlyWage = 23,  SalaryAmount = Salary(23),  MaxSlots = -1,
                Description = "Repairs city-owned infrastructure." },
            new() { Id = "road_worker",        DisplayName = "Road Worker",        Department = "Trades",     Tier = JobTier.Free, HourlyWage = 22,  SalaryAmount = Salary(22),  MaxSlots = -1,
                Description = "Paves and repairs roads and sidewalks." },
            new() { Id = "sanitation_worker",  DisplayName = "Sanitation Worker",  Department = "Trades",     Tier = JobTier.Free, HourlyWage = 24,  SalaryAmount = Salary(24),  MaxSlots = -1,
                Description = "Collects garbage. Keeps the city clean." },
            new() { Id = "recycler",           DisplayName = "Recycler",           Department = "Trades",     Tier = JobTier.Free, HourlyWage = 17,  SalaryAmount = Salary(17),  MaxSlots = -1,
                Description = "Processes recyclable materials for cash." },
            new() { Id = "scrap_collector",    DisplayName = "Scrap Collector",    Department = "Trades",     Tier = JobTier.Free, HourlyWage = 15,  SalaryAmount = Salary(15),  MaxSlots = -1,
                Description = "Gathers scrap metal and salvage." },
            new() { Id = "server_technician",  DisplayName = "Server Technician",  Department = "Trades",     Tier = JobTier.Free, HourlyWage = 29,  SalaryAmount = Salary(29),  MaxSlots = -1,
                Description = "Maintains server hardware in data centers." },
            new() { Id = "it_technician",      DisplayName = "IT Technician",      Department = "Trades",     Tier = JobTier.Free, HourlyWage = 27,  SalaryAmount = Salary(27),  MaxSlots = -1,
                Description = "Fixes computers and networks." },
            new() { Id = "network_engineer",   DisplayName = "Network Engineer",   Department = "Trades",     Tier = JobTier.Vip,  HourlyWage = 40,  SalaryAmount = Salary(40),  MaxSlots = 2,
                Description = "Designs and manages network infrastructure." },
            new() { Id = "civil_engineer",     DisplayName = "Civil Engineer",     Department = "Trades",     Tier = JobTier.Vip,  HourlyWage = 38,  SalaryAmount = Salary(38),  MaxSlots = 2,
                Description = "Plans roads, bridges, and public works." },
            new() { Id = "structural_engineer",DisplayName = "Structural Engineer",Department = "Trades",     Tier = JobTier.Vip,  HourlyWage = 40,  SalaryAmount = Salary(40),  MaxSlots = 2,
                Description = "Ensures buildings don't fall down." },
            new() { Id = "transit_maintenance",DisplayName = "Transit Maintenance Worker",Department = "Trades",Tier = JobTier.Free,HourlyWage = 25, SalaryAmount = Salary(25),  MaxSlots = -1,
                Description = "Keeps buses and trains running." },

            // ─────────────────────────────────────────────────────────────────
            // CATEGORY D — Medical / Science / Education
            // ─────────────────────────────────────────────────────────────────
            new() { Id = "emt",                DisplayName = "EMT",                Department = "Medical",    Tier = JobTier.Free, HourlyWage = 20,  SalaryAmount = Salary(20),  MaxSlots = -1,
                Description = "First on scene. Stabilizes the injured." },
            new() { Id = "paramedic",          DisplayName = "Paramedic",          Department = "Medical",    Tier = JobTier.Free, HourlyWage = 24,  SalaryAmount = Salary(24),  MaxSlots = -1,
                Description = "Advanced emergency care in the field." },
            new() { Id = "nurse",              DisplayName = "Nurse",              Department = "Medical",    Tier = JobTier.Free, HourlyWage = 36,  SalaryAmount = Salary(36),  MaxSlots = -1,
                Description = "Provides patient care at clinics and hospitals." },
            new() { Id = "doctor",             DisplayName = "Doctor",             Department = "Medical",    Tier = JobTier.Free, HourlyWage = 70,  SalaryAmount = Salary(70),  MaxSlots = 4,
                Description = "Diagnoses and treats patients." },
            new() { Id = "surgeon",            DisplayName = "Surgeon",            Department = "Medical",    Tier = JobTier.Vip,  HourlyWage = 95,  SalaryAmount = Salary(95),  MaxSlots = 2,
                Description = "Performs surgical procedures." },
            new() { Id = "trauma_surgeon",     DisplayName = "Trauma Surgeon",     Department = "Medical",    Tier = JobTier.Vip,  HourlyWage = 110, SalaryAmount = Salary(110), MaxSlots = 1,
                Description = "Critical trauma specialist. Saves lives in the ER." },
            new() { Id = "chief_surgeon",      DisplayName = "Chief Surgeon",      Department = "Medical",    Tier = JobTier.Vip,  HourlyWage = 110, SalaryAmount = Salary(110), MaxSlots = 1,
                Description = "Head of surgery. Highest medical authority in the hospital." },
            new() { Id = "pharmacist",         DisplayName = "Pharmacist",         Department = "Medical",    Tier = JobTier.Free, HourlyWage = 58,  SalaryAmount = Salary(58),  MaxSlots = 2,
                Description = "Dispenses medication. Knows what every pill does." },
            new() { Id = "therapist",          DisplayName = "Therapist",          Department = "Medical",    Tier = JobTier.Free, HourlyWage = 30,  SalaryAmount = Salary(30),  MaxSlots = -1,
                Description = "Mental health support for stressed city residents." },
            new() { Id = "coroner",            DisplayName = "Coroner",            Department = "Medical",    Tier = JobTier.Free, HourlyWage = 32,  SalaryAmount = Salary(32),  MaxSlots = 2,
                Description = "Investigates deaths. Sees the aftermath of crime." },
            new() { Id = "lab_technician",     DisplayName = "Lab Technician",     Department = "Medical",    Tier = JobTier.Free, HourlyWage = 24,  SalaryAmount = Salary(24),  MaxSlots = -1,
                Description = "Runs lab tests and processes samples." },
            new() { Id = "lab_assistant",      DisplayName = "Lab Assistant",      Department = "Medical",    Tier = JobTier.Free, HourlyWage = 20,  SalaryAmount = Salary(20),  MaxSlots = -1,
                Description = "Supports lab technicians with routine tasks." },
            new() { Id = "researcher",         DisplayName = "Researcher",         Department = "Medical",    Tier = JobTier.Free, HourlyWage = 32,  SalaryAmount = Salary(32),  MaxSlots = -1,
                Description = "Conducts experiments and documents findings." },
            new() { Id = "scientist",          DisplayName = "Scientist",          Department = "Medical",    Tier = JobTier.Free, HourlyWage = 38,  SalaryAmount = Salary(38),  MaxSlots = -1,
                Description = "Advanced researcher in any scientific field." },
            new() { Id = "senior_scientist",   DisplayName = "Senior Scientist",   Department = "Medical",    Tier = JobTier.Vip,  HourlyWage = 45,  SalaryAmount = Salary(45),  MaxSlots = 2,
                Description = "Leads research teams and high-level experiments." },
            new() { Id = "teacher",            DisplayName = "Teacher",            Department = "Education",  Tier = JobTier.Free, HourlyWage = 28,  SalaryAmount = Salary(28),  MaxSlots = -1,
                Description = "Educates students. Respectable member of the community." },
            new() { Id = "professor",          DisplayName = "Professor",          Department = "Education",  Tier = JobTier.Free, HourlyWage = 36,  SalaryAmount = Salary(36),  MaxSlots = -1,
                Description = "University-level educator and researcher." },
            new() { Id = "hospital_director",  DisplayName = "Hospital Director",  Department = "Medical",    Tier = JobTier.Vip,  HourlyWage = 58,  SalaryAmount = Salary(58),  MaxSlots = 1,
                Description = "Runs the entire hospital. Final word on medical policy." },

            // ─────────────────────────────────────────────────────────────────
            // CATEGORY E — Transit / Travel / Logistics
            // ─────────────────────────────────────────────────────────────────
            new() { Id = "delivery_driver",    DisplayName = "Delivery Driver",    Department = "Logistics",  Tier = JobTier.Free, HourlyWage = 19,  SalaryAmount = Salary(19),  MaxSlots = -1,
                Description = "Delivers packages and goods across the city." },
            new() { Id = "taxi_driver",        DisplayName = "Taxi Driver",        Department = "Logistics",  Tier = JobTier.Free, HourlyWage = 20,  SalaryAmount = Salary(20),  MaxSlots = -1,
                Description = "Transports passengers for fare." },
            new() { Id = "rideshare_driver",   DisplayName = "Rideshare Driver",   Department = "Logistics",  Tier = JobTier.Free, HourlyWage = 20,  SalaryAmount = Salary(20),  MaxSlots = -1,
                Description = "App-based ride service. Flexible hours." },
            new() { Id = "bus_driver",         DisplayName = "Bus Driver",         Department = "Logistics",  Tier = JobTier.Free, HourlyWage = 26,  SalaryAmount = Salary(26),  MaxSlots = -1,
                Description = "Operates city transit buses on fixed routes." },
            new() { Id = "train_driver",       DisplayName = "Train Driver",       Department = "Logistics",  Tier = JobTier.Free, HourlyWage = 32,  SalaryAmount = Salary(32),  MaxSlots = -1,
                Description = "Operates commuter or freight trains." },
            new() { Id = "bart_operator",      DisplayName = "BART Operator",      Department = "Logistics",  Tier = JobTier.Free, HourlyWage = 35,  SalaryAmount = Salary(35),  MaxSlots = -1,
                Description = "Runs BART trains across the Bay Area network." },
            new() { Id = "freight_conductor",  DisplayName = "Freight Conductor",  Department = "Logistics",  Tier = JobTier.Free, HourlyWage = 30,  SalaryAmount = Salary(30),  MaxSlots = -1,
                Description = "Manages freight rail operations." },
            new() { Id = "pilot",              DisplayName = "Pilot",              Department = "Logistics",  Tier = JobTier.Vip,  HourlyWage = 45,  SalaryAmount = Salary(45),  MaxSlots = 2,
                Description = "Flies passenger aircraft." },
            new() { Id = "cargo_pilot",        DisplayName = "Cargo Pilot",        Department = "Logistics",  Tier = JobTier.Vip,  HourlyWage = 50,  SalaryAmount = Salary(50),  MaxSlots = 2,
                Description = "Transports freight by air." },
            new() { Id = "helicopter_pilot",   DisplayName = "Helicopter Pilot",   Department = "Logistics",  Tier = JobTier.Vip,  HourlyWage = 50,  SalaryAmount = Salary(50),  MaxSlots = 2,
                Description = "Aerial transport and surveillance." },
            new() { Id = "dock_worker",        DisplayName = "Dock Worker",        Department = "Logistics",  Tier = JobTier.Free, HourlyWage = 25,  SalaryAmount = Salary(25),  MaxSlots = -1,
                Description = "Loads and unloads ships at the port." },
            new() { Id = "longshoreman",       DisplayName = "Longshoreman",       Department = "Logistics",  Tier = JobTier.Free, HourlyWage = 26,  SalaryAmount = Salary(26),  MaxSlots = -1,
                Description = "Union port worker. Controls the docks." },
            new() { Id = "fisherman",          DisplayName = "Fisherman",          Department = "Logistics",  Tier = JobTier.Free, HourlyWage = 18,  SalaryAmount = Salary(18),  MaxSlots = -1,
                Description = "Catches fish for sale or survival." },
            new() { Id = "trucker",            DisplayName = "Trucker",            Department = "Logistics",  Tier = JobTier.Free, HourlyWage = 28,  SalaryAmount = Salary(28),  MaxSlots = -1,
                Description = "Hauls freight across the city and region." },
            new() { Id = "logistics_coordinator",DisplayName = "Logistics Coordinator",Department = "Logistics",Tier = JobTier.Free,HourlyWage = 28, SalaryAmount = Salary(28),  MaxSlots = -1,
                Description = "Coordinates supply chain and delivery schedules." },
            new() { Id = "dispatcher",         DisplayName = "Dispatcher",         Department = "Logistics",  Tier = JobTier.Free, HourlyWage = 24,  SalaryAmount = Salary(24),  MaxSlots = -1,
                Description = "Routes drivers, taxis, and emergency units." },
            new() { Id = "fuel_transport_driver",DisplayName = "Fuel Transport Driver",Department = "Logistics",Tier = JobTier.Free,HourlyWage = 30, SalaryAmount = Salary(30),  MaxSlots = -1,
                Description = "Hauls fuel and hazardous materials. CDL required." },

            // ─────────────────────────────────────────────────────────────────
            // CATEGORY F — Crime / Street / Underworld
            // Crime jobs earn $0 legal wage. Income comes from crime payouts.
            // ─────────────────────────────────────────────────────────────────
            new() { Id = "hobo",               DisplayName = "Hobo",               Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "No home, no income, no rules. Survive the streets." },
            new() { Id = "hobo_king",          DisplayName = "Hobo King",          Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = 1,
                Description = "Rules the homeless. Respected by those with nothing." },
            new() { Id = "beggar",             DisplayName = "Beggar",             Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Asks for handouts. Better at it than you'd expect." },
            new() { Id = "pickpocket",         DisplayName = "Pickpocket",         Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Lifts wallets without being noticed." },
            new() { Id = "mugger",             DisplayName = "Mugger",             Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Takes valuables by force on the street." },
            new() { Id = "thief",              DisplayName = "Thief",              Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Steals from businesses and players." },
            new() { Id = "burglar",            DisplayName = "Burglar",            Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Breaks into buildings and properties." },
            new() { Id = "bandit",             DisplayName = "Bandit",             Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Ambushes travellers and delivery drivers." },
            new() { Id = "robber",             DisplayName = "Robber",             Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Armed robbery. High risk, high reward." },
            new() { Id = "car_thief",          DisplayName = "Car Thief",          Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Steals vehicles and strips them for parts." },
            new() { Id = "chop_shop_worker",   DisplayName = "Chop Shop Worker",   Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Dismantles stolen vehicles for cash." },
            new() { Id = "safe_cracker",       DisplayName = "Safe Cracker",       Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Opens safes without a key. Patience and skill." },
            new() { Id = "lockbreaker",        DisplayName = "Lockbreaker",        Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Forces locks. Fast and loud." },
            new() { Id = "hacker",             DisplayName = "Hacker",             Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Infiltrates systems for money or chaos." },
            new() { Id = "advanced_hacker",    DisplayName = "Advanced Hacker",    Department = "Crime",      Tier = JobTier.Vip,  HourlyWage = 0, SalaryAmount = 0, MaxSlots = 2,
                Description = "Elite-level intrusion. Can breach government systems." },
            new() { Id = "scammer",            DisplayName = "Scammer",            Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Runs cons and fraud schemes on other players." },
            new() { Id = "smuggler",           DisplayName = "Smuggler",           Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Moves contraband past checkpoints." },
            new() { Id = "drug_runner",        DisplayName = "Drug Runner",        Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Ferries product between suppliers and distributors." },
            new() { Id = "fence",              DisplayName = "Fence",              Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Buys and resells stolen goods." },
            new() { Id = "black_market_dealer",DisplayName = "Black Market Dealer",Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Sells illegal goods off the books." },
            new() { Id = "hitman",             DisplayName = "Hitman",             Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Takes contracts on players. No witnesses." },
            new() { Id = "mercenary",          DisplayName = "Mercenary",          Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Guns for hire. Works for whoever pays." },
            new() { Id = "enforcer",           DisplayName = "Enforcer",           Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Collects debts and delivers messages with force." },
            new() { Id = "gang_recruit",       DisplayName = "Gang Recruit",       Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Newest member of a street gang. Proving themselves." },
            new() { Id = "gang_member",        DisplayName = "Gang Member",        Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Full member of a street gang." },
            new() { Id = "gang_shotcaller",    DisplayName = "Gang Shotcaller",    Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = 2,
                Description = "Calls the shots for a street gang." },
            new() { Id = "mob_associate",      DisplayName = "Mob Associate",      Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Connected to organized crime. Not yet fully in." },
            new() { Id = "mafia_soldier",      DisplayName = "Mafia Soldier",      Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Sworn member of the mafia. Does what they're told." },
            new() { Id = "mafia_capo",         DisplayName = "Mafia Capo",         Department = "Crime",      Tier = JobTier.Vip,  HourlyWage = 0, SalaryAmount = 0, MaxSlots = 2,
                Description = "Commands a mafia crew. Answers only to the boss." },
            new() { Id = "cartel_runner",      DisplayName = "Cartel Runner",      Department = "Crime",      Tier = JobTier.Free, HourlyWage = 0, SalaryAmount = 0, MaxSlots = -1,
                Description = "Moves product for the cartel." },
            new() { Id = "cartel_boss",        DisplayName = "Cartel Boss",        Department = "Crime",      Tier = JobTier.Vip,  HourlyWage = 0, SalaryAmount = 0, MaxSlots = 1,
                Description = "Controls cartel operations in the region." },
            new() { Id = "kingpin",            DisplayName = "Kingpin",            Department = "Crime",      Tier = JobTier.Vip,  HourlyWage = 0, SalaryAmount = 0, MaxSlots = 1,
                Description = "Top of the criminal hierarchy. Untouchable — until they aren't." },
            new() { Id = "premium_mercenary",  DisplayName = "Premium Mercenary",  Department = "Crime",      Tier = JobTier.Vip,  HourlyWage = 0, SalaryAmount = 0, MaxSlots = 2,
                Description = "Elite hired gun. Better kit, bigger contracts." },
            new() { Id = "elite_thief",        DisplayName = "Elite Thief",        Department = "Crime",      Tier = JobTier.Vip,  HourlyWage = 0, SalaryAmount = 0, MaxSlots = 2,
                Description = "Master-level thief. Can breach any location." },
            new() { Id = "master_locksmith",   DisplayName = "Master Locksmith",   Department = "Crime",      Tier = JobTier.Vip,  HourlyWage = 0, SalaryAmount = 0, MaxSlots = 2,
                Description = "Opens any lock. Legal or otherwise." },
            new() { Id = "hybrid_infiltrator", DisplayName = "Hybrid Infiltrator", Department = "Crime",      Tier = JobTier.Vip,  HourlyWage = 0, SalaryAmount = 0, MaxSlots = 1,
                Description = "Can blend into civilian or criminal roles. The ultimate spy." },

            // ─────────────────────────────────────────────────────────────────
            // CATEGORY G — Police / Security / Government Force
            // ─────────────────────────────────────────────────────────────────
            new() { Id = "mall_security",      DisplayName = "Mall Security",      Department = "Security",   Tier = JobTier.Free, HourlyWage = 18,  SalaryAmount = Salary(18),  MaxSlots = -1,
                Description = "Patrols the mall. Takes it very seriously." },
            new() { Id = "security_guard",     DisplayName = "Security Guard",     Department = "Security",   Tier = JobTier.Free, HourlyWage = 18,  SalaryAmount = Salary(18),  MaxSlots = -1,
                Description = "Protects a building or business." },
            new() { Id = "armed_guard",        DisplayName = "Armed Guard",        Department = "Security",   Tier = JobTier.Free, HourlyWage = 24,  SalaryAmount = Salary(24),  MaxSlots = -1,
                Description = "Security with a license to carry." },
            new() { Id = "bouncer",            DisplayName = "Bouncer",            Department = "Security",   Tier = JobTier.Free, HourlyWage = 18,  SalaryAmount = Salary(18),  MaxSlots = -1,
                Description = "Controls entry to clubs and venues." },
            new() { Id = "patrol_officer",     DisplayName = "Patrol Officer",     Department = "Police",     Tier = JobTier.Free, HourlyWage = 34,  SalaryAmount = Salary(34),  MaxSlots = -1,
                CanArrest = true, CanSetWanted = true,
                Description = "Street-level law enforcement. First to respond." },
            new() { Id = "traffic_officer",    DisplayName = "Traffic Officer",    Department = "Police",     Tier = JobTier.Free, HourlyWage = 30,  SalaryAmount = Salary(30),  MaxSlots = -1,
                CanArrest = true, CanSetWanted = true,
                Description = "Enforces traffic laws and manages road incidents." },
            new() { Id = "transit_police",     DisplayName = "Transit Police",     Department = "Police",     Tier = JobTier.Free, HourlyWage = 34,  SalaryAmount = Salary(34),  MaxSlots = -1,
                CanArrest = true, CanSetWanted = true,
                Description = "Polices buses, trains, and stations." },
            new() { Id = "sheriff_deputy",     DisplayName = "Sheriff Deputy",     Department = "Police",     Tier = JobTier.Free, HourlyWage = 33,  SalaryAmount = Salary(33),  MaxSlots = -1,
                CanArrest = true, CanSetWanted = true,
                Description = "County law enforcement." },
            new() { Id = "detective",          DisplayName = "Detective",          Department = "Police",     Tier = JobTier.Free, HourlyWage = 38,  SalaryAmount = Salary(38),  MaxSlots = -1,
                CanArrest = true, CanSetWanted = true,
                Description = "Investigates crimes and builds cases." },
            new() { Id = "investigator",       DisplayName = "Investigator",       Department = "Police",     Tier = JobTier.Free, HourlyWage = 35,  SalaryAmount = Salary(35),  MaxSlots = -1,
                CanArrest = true, CanSetWanted = true,
                Description = "Digs deep into criminal networks." },
            new() { Id = "swat_officer",       DisplayName = "SWAT Officer",       Department = "Police",     Tier = JobTier.Free, HourlyWage = 40,  SalaryAmount = Salary(40),  MaxSlots = 4,
                CanArrest = true, CanSetWanted = true,
                Description = "Tactical response team. Called for high-risk situations." },
            new() { Id = "swat_elite",         DisplayName = "SWAT Elite",         Department = "Police",     Tier = JobTier.Vip,  HourlyWage = 46,  SalaryAmount = Salary(46),  MaxSlots = 2,
                CanArrest = true, CanSetWanted = true,
                Description = "Elite tier SWAT. Better kit, command authority." },
            new() { Id = "swat_breacher",      DisplayName = "SWAT Breacher",      Department = "Police",     Tier = JobTier.Free, HourlyWage = 40,  SalaryAmount = Salary(40),  MaxSlots = 2,
                CanArrest = true, CanSetWanted = true,
                Description = "First through the door. Breach and clear." },
            new() { Id = "prison_guard",       DisplayName = "Prison Guard",       Department = "Police",     Tier = JobTier.Free, HourlyWage = 25,  SalaryAmount = Salary(25),  MaxSlots = -1,
                CanArrest = true,
                Description = "Maintains order inside the prison." },
            new() { Id = "corrections_officer",DisplayName = "Corrections Officer",Department = "Police",     Tier = JobTier.Free, HourlyWage = 24,  SalaryAmount = Salary(24),  MaxSlots = -1,
                CanArrest = true,
                Description = "Manages incarcerated players." },
            new() { Id = "bailiff",            DisplayName = "Bailiff",            Department = "Police",     Tier = JobTier.Free, HourlyWage = 30,  SalaryAmount = Salary(30),  MaxSlots = -1,
                CanArrest = true,
                Description = "Maintains order in the courtroom." },
            new() { Id = "warden",             DisplayName = "Warden",             Department = "Police",     Tier = JobTier.Vip,  HourlyWage = 35,  SalaryAmount = Salary(35),  MaxSlots = 1,
                CanArrest = true, CanSetWanted = true,
                Description = "Runs the prison. Authority over guards and inmates." },
            new() { Id = "bodyguard",          DisplayName = "Bodyguard",          Department = "Security",   Tier = JobTier.Free, HourlyWage = 28,  SalaryAmount = Salary(28),  MaxSlots = -1,
                Description = "Personal protection for hire." },
            new() { Id = "executive_protection",DisplayName = "Executive Protection Agent",Department = "Security",Tier = JobTier.Vip,HourlyWage = 40,SalaryAmount = Salary(40),MaxSlots = 4,
                Description = "High-level protection detail for VIPs and officials." },
            new() { Id = "secret_service",     DisplayName = "Secret Service Agent",Department = "Federal",   Tier = JobTier.Vip,  HourlyWage = 48,  SalaryAmount = Salary(48),  MaxSlots = 4,
                CanArrest = true, CanSetWanted = true,
                Description = "Protects the President and senior government officials." },
            new() { Id = "federal_marshal",    DisplayName = "Federal Marshal",    Department = "Federal",    Tier = JobTier.Vip,  HourlyWage = 43,  SalaryAmount = Salary(43),  MaxSlots = 3,
                CanArrest = true, CanSetWanted = true,
                Description = "Federal law enforcement. Jurisdiction everywhere." },
            new() { Id = "federal_marshal_elite",DisplayName = "Federal Marshal Elite",Department = "Federal",Tier = JobTier.Vip,  HourlyWage = 52,  SalaryAmount = Salary(52),  MaxSlots = 1,
                CanArrest = true, CanSetWanted = true,
                Description = "Top-ranked federal marshal. Commands field operations." },
            new() { Id = "fbi_agent",          DisplayName = "FBI Agent",          Department = "Federal",    Tier = JobTier.Vip,  HourlyWage = 41,  SalaryAmount = Salary(41),  MaxSlots = 4,
                CanArrest = true, CanSetWanted = true,
                Description = "Federal investigation. Pursues organized crime." },
            new() { Id = "fbi_supervisor",     DisplayName = "FBI Supervisor",     Department = "Federal",    Tier = JobTier.Vip,  HourlyWage = 50,  SalaryAmount = Salary(50),  MaxSlots = 2,
                CanArrest = true, CanSetWanted = true,
                Description = "Commands FBI field teams." },
            new() { Id = "cia_officer",        DisplayName = "CIA Officer",        Department = "Federal",    Tier = JobTier.Vip,  HourlyWage = 42,  SalaryAmount = Salary(42),  MaxSlots = 3,
                Description = "Intelligence operations. Off the record." },
            new() { Id = "cia_handler",        DisplayName = "CIA Handler",        Department = "Federal",    Tier = JobTier.Vip,  HourlyWage = 48,  SalaryAmount = Salary(48),  MaxSlots = 2,
                Description = "Manages assets and covert operations." },
            new() { Id = "homeland_security",  DisplayName = "Homeland Security Agent",Department = "Federal", Tier = JobTier.Vip,  HourlyWage = 40,  SalaryAmount = Salary(40),  MaxSlots = 3,
                CanArrest = true, CanSetWanted = true,
                Description = "Prevents large-scale threats to the city." },
            new() { Id = "internal_affairs",   DisplayName = "Internal Affairs Officer",Department = "Police", Tier = JobTier.Free, HourlyWage = 39,  SalaryAmount = Salary(39),  MaxSlots = 2,
                CanArrest = true, CanSetWanted = true,
                Description = "Investigates corruption inside the police force." },

            // ─────────────────────────────────────────────────────────────────
            // CATEGORY H — Politics / Leadership / Elite Roles
            // ─────────────────────────────────────────────────────────────────
            new() { Id = "council_member",     DisplayName = "Council Member",     Department = "Government", Tier = JobTier.Vip,  HourlyWage = 38,  SalaryAmount = Salary(38),  MaxSlots = 4,
                CanSetWanted = true,
                Description = "City council seat. Votes on local laws." },
            new() { Id = "city_manager",       DisplayName = "City Manager",       Department = "Government", Tier = JobTier.Vip,  HourlyWage = 48,  SalaryAmount = Salary(48),  MaxSlots = 1,
                CanSetWanted = true,
                Description = "Administers city operations day to day." },
            new() { Id = "district_attorney",  DisplayName = "District Attorney",  Department = "Government", Tier = JobTier.Vip,  HourlyWage = 50,  SalaryAmount = Salary(50),  MaxSlots = 1,
                CanArrest = true, CanSetWanted = true,
                Description = "Prosecutes criminal cases. Decides who gets charged." },
            new() { Id = "judge",              DisplayName = "Judge",              Department = "Government", Tier = JobTier.Vip,  HourlyWage = 65,  SalaryAmount = Salary(65),  MaxSlots = 1,
                CanArrest = true, CanSetWanted = true,
                Description = "Presides over court. Final say on sentencing." },
            new() { Id = "mayor",              DisplayName = "Mayor",              Department = "Government", Tier = JobTier.Vip,  HourlyWage = 42,  SalaryAmount = Salary(42),  MaxSlots = 1,
                CanSetWanted = true,
                Description = "Leader of the city. Sets tax rate and city laws." },
            new() { Id = "governor_liaison",   DisplayName = "Governor Liaison",   Department = "Government", Tier = JobTier.Vip,  HourlyWage = 55,  SalaryAmount = Salary(55),  MaxSlots = 1,
                Description = "Represents state-level authority in the city." },
            new() { Id = "president",          DisplayName = "President",          Department = "Government", Tier = JobTier.Vip,  HourlyWage = 96,  SalaryAmount = Salary(96),  MaxSlots = 1,
                CanSetWanted = true,
                Description = "Highest authority. Commanding presence. Constant target." },
            new() { Id = "vice_president",     DisplayName = "Vice President",     Department = "Government", Tier = JobTier.Vip,  HourlyWage = 70,  SalaryAmount = Salary(70),  MaxSlots = 1,
                CanSetWanted = true,
                Description = "Second in command. Steps up if the President falls." },
            new() { Id = "chief_of_staff",     DisplayName = "Chief of Staff",     Department = "Government", Tier = JobTier.Vip,  HourlyWage = 58,  SalaryAmount = Salary(58),  MaxSlots = 1,
                Description = "Manages the President's agenda and inner circle." },
            new() { Id = "press_secretary",    DisplayName = "Press Secretary",    Department = "Government", Tier = JobTier.Vip,  HourlyWage = 40,  SalaryAmount = Salary(40),  MaxSlots = 1,
                Description = "Handles official communication for the administration." },
            new() { Id = "intelligence_director",DisplayName = "Intelligence Director",Department = "Federal",Tier = JobTier.Vip,  HourlyWage = 60,  SalaryAmount = Salary(60),  MaxSlots = 1,
                Description = "Oversees all intelligence and covert operations." },
            new() { Id = "police_chief",       DisplayName = "Police Chief",       Department = "Police",     Tier = JobTier.Vip,  HourlyWage = 52,  SalaryAmount = Salary(52),  MaxSlots = 1,
                CanArrest = true, CanSetWanted = true,
                Description = "Heads the entire police department." },
            new() { Id = "commissioner",       DisplayName = "Commissioner",       Department = "Police",     Tier = JobTier.Vip,  HourlyWage = 52,  SalaryAmount = Salary(52),  MaxSlots = 1,
                CanArrest = true, CanSetWanted = true,
                Description = "Top oversight authority over law enforcement." },
            new() { Id = "university_dean",    DisplayName = "University Dean",    Department = "Education",  Tier = JobTier.Vip,  HourlyWage = 55,  SalaryAmount = Salary(55),  MaxSlots = 1,
                Description = "Heads the university. Academic authority." },
        };

        _jobs = list.ToDictionary( j => j.Id );
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────────────

    public static JobDefinition Get( string id )
        => _jobs.TryGetValue( id, out var job ) ? job : _jobs["citizen"];

    public static bool Exists( string id )
        => _jobs.ContainsKey( id );

    public static IReadOnlyCollection<JobDefinition> All
        => _jobs.Values;

    public static IEnumerable<JobDefinition> ByTier( JobTier tier )
        => _jobs.Values.Where( j => j.Tier == tier );

    public static IEnumerable<JobDefinition> ByDepartment( string department )
        => _jobs.Values.Where( j => j.Department.Equals( department, StringComparison.OrdinalIgnoreCase ) );

    /// <summary>Returns how many players currently hold this job.</summary>
    public static int CountPlayersInJob( string jobId )
        => Game.ActiveScene?
            .GetAllComponents<PlayerState>()
            .Count( s => s.JobId == jobId ) ?? 0;

    /// <summary>Returns false if the job is full.</summary>
    public static bool IsJobAvailable( string jobId )
    {
        var def = Get( jobId );
        if ( def.MaxSlots < 0 ) return true;
        return CountPlayersInJob( jobId ) < def.MaxSlots;
    }

    /// <summary>Returns false if this job requires VIP and the player is not VIP.</summary>
    public static bool IsJobUnlockedFor( string jobId, bool playerIsVip )
    {
        var def = Get( jobId );
        return def.Tier == JobTier.Free || playerIsVip;
    }
}
