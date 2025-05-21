using KıbrısApp3.Data;
using KıbrısApp3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KıbrısApp3.Controllers
{
    [Route("api/vehicle-categories")]
    [ApiController]
    public class VehicleCategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehicleCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetVehicleCategoryTree()
        {
            var vasita = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Vasıta");
            if (vasita == null)
                return NotFound("Vasıta kategorisi bulunamadı.");

            var allCategories = await _context.Categories.ToListAsync();

            var categoryDict = allCategories.ToDictionary(c => c.Id);
            foreach (var cat in allCategories)
            {
                cat.Children = new List<Category>();
            }

            foreach (var category in allCategories)
            {
                if (category.ParentCategoryId.HasValue &&
                    categoryDict.ContainsKey(category.ParentCategoryId.Value))
                {
                    categoryDict[category.ParentCategoryId.Value].Children.Add(category);
                }
            }

            var vasitaCategoryTree = BuildCategoryDto(categoryDict[vasita.Id]);

            return Ok(vasitaCategoryTree);
        }

        private object BuildCategoryDto(Category category)
        {
            return new
            {
                id = category.Id,
                name = category.Name,
                children = category.Children?.Select(BuildCategoryDto).ToList()
            };
        }


        [HttpPost("seed-details")]
        public async Task<IActionResult> SeedVehicleCategoryDetails()
        {
            var otomobil = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Otomobil");
            var suv = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Arazi-SUV-Pick-Up");

            if (otomobil == null || suv == null)
                return BadRequest("Otomobil veya SUV kategorisi eksik.");

            var otomobilData = GetOtomobilData();
            var suvData = GetSuvData();

            foreach (var marka in otomobilData)
            {
                var markaEntity = new Category { Name = marka.Key, ParentCategoryId = otomobil.Id };
                await _context.Categories.AddAsync(markaEntity);
                await _context.SaveChangesAsync();

                foreach (var model in marka.Value)
                {
                    await _context.Categories.AddAsync(new Category
                    {
                        Name = model,
                        ParentCategoryId = markaEntity.Id
                    });
                }
            }

            foreach (var marka in suvData)
            {
                var markaEntity = new Category { Name = marka.Key, ParentCategoryId = suv.Id };
                await _context.Categories.AddAsync(markaEntity);
                await _context.SaveChangesAsync();

                foreach (var model in marka.Value)
                {
                    await _context.Categories.AddAsync(new Category
                    {
                        Name = model,
                        ParentCategoryId = markaEntity.Id
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Vasıta altındaki marka ve model detayları başarıyla eklendi.");
        }


        // ✅ Seed motor marka + model kategori ağacı
        [HttpPost("seed-motorcycles")]
        public async Task<IActionResult> SeedMotorcycleCategories()
        {
            var motosikletKategori = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == "Motosiklet");

            if (motosikletKategori == null)
                return BadRequest("Önce 'Motosiklet' ana kategorisi oluşturulmalı!");

            var data = GetMotorcycleData();
            var all = new List<Category>();

            foreach (var (brand, models) in data)
            {
                var brandCategory = new Category
                {
                    Name = brand,
                    ParentCategoryId = motosikletKategori.Id
                };
                all.Add(brandCategory);
                await _context.Categories.AddAsync(brandCategory);
                await _context.SaveChangesAsync();

                var modelCategories = models.Select(m => new Category
                {
                    Name = m,
                    ParentCategoryId = brandCategory.Id
                });

                all.AddRange(modelCategories);
                await _context.Categories.AddRangeAsync(modelCategories);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Tüm motosiklet markaları ve modelleri başarıyla eklendi!", count = all.Count });
        }

        // ✅ Motosiklet veri kaynağı (dictionary)
        private Dictionary<string, List<string>> GetMotorcycleData()
        {
            return new Dictionary<string, List<string>>
            {
                { "ALTAI", new List<string> { "Diğer" } },
                { "APACHI", new List<string> { "Diğer" } },
                { "APRILLIA", new List<string> { "Amico 50", "Atlantic 200", "Atlantic 500 Sprint", "Caponord 1000 ETV", "Caponord 1000 ETV ABS", "Caponord 1200 ABS Travel", "RS125", "RS660", "RS4 125 4T", "RSV4 1100", "RSV4 Factory", "RSV4 RF", "Diğer" } },
                { "ARORA", new List<string> { "Diğer" } },
                { "ASYA", new List<string> { "Diğer" } },
                { "BAJAJ", new List<string> { "Pulsar 200NS", "Pulsar 200RS" } },
                { "BENELLI", new List<string> { "125-S", "502-C", "BN 125", "BN 251", "TNT 125", "TNT 249-S", "TNT 25", "TNT 250", "TNT 600", "TRK 251", "TRK 502", "TRK 502 X" } },
                { "BMW", new List<string> { "C 400 GT", "C 400 X", "C 600 Sport", "C 650 GT", "C1", "F 650", "F 650 CS", "F 650 GS", "F 650 GS Dakar", "F 650 ST", "F 700 GS", "F 750 GS", "F 800 GS", "F 800 GS Adventure", "F 800 R", "F 800 S", "F 850 GS", "F 900 XR", "G 310 GS", "G 310 R", "G 450 X", "G 650 GS", "G 650 X Country", "K 100 LT", "K 1200 GT", "K 1200LT", "K 1200 R", "K 1200 RS", "K 1200 S", "K 1300 GT", "K 1300 R", "K 1600 Bagger", "K 1600 Grand America", "K 1600 GT", "K 1600 GTL", "K 1600 GTL Exculusive", "K1", "R 100 R", "R 1100 GS", "R 1100 RS", "R 1100 RT", "R 1150 GS", "R 1150 GS Adventure", "R 1150 R", "R 1150 RT", "R 1200 C", "R 1200 CL", "R 1200 GS", "R 1200 GS Adventure", "R 1250 RT", "R 1300 GS", "R Nine T", "R Nine T Scrambler", "R Nine T Urban G/S", "S 1000 R", "S 1000 RR", "S 1000 RR HP", "S 1000 XR" } },
                { "CF MOTO", new List<string> { "150NK", "250NK", "250SR", "400NK", "450NK", "450SR", "650MT", "650NK" } },
                { "CHOPPER", new List<string> { "Diğer" } },
                { "DUCATI", new List<string> { "Diavel", "Monster 821", "Multistrada 1200", "Panigale V4", "Scrambler 1100", "Diğer" } },
                { "FALCON", new List<string> { "Diğer" } },
                { "GILERA", new List<string> { "Diğer" } },
                { "HARLEY DAVIDSON", new List<string> { "Diğer" } },
                { "HONDA", new List<string> { "Activa 125", "CBR600RR", "CBR 1000 RR", "CRF 250 Rally", "CRF 1000L Africa Twin", "Diğer" } },
                { "HYOSUNG", new List<string> { "Diğer" } },
                { "JAWA", new List<string> { "Diğer" } },
                { "KAWASAKI", new List<string> { "Ninja 300", "Ninja ZX-10 R", "Z 900", "Z 1000", "Versys X300", "Diğer" } },
                { "KTM", new List<string> { "RC 125", "RC 390", "Duke 200", "Duke 390", "1190 Adventure", "Diğer" } },
                { "MONDIAL", new List<string> { "Diğer" } },
                { "RKS", new List<string> { "125 R", "Azure 50", "Bitter", "Dark Blue", "Diğer" } },
                { "SUZUKI", new List<string> { "GSX-R 125", "GSX-R 600", "GSX-R 1000", "V-STORM 800", "Diğer" } },
                { "SYM", new List<string> { "Diğer" } },
                { "TRIUMPH", new List<string> { "Boneville T100", "Daytona 660", "Scrambler", "Speed Triple", "Diğer" } },
                { "VESPA", new List<string> { "Diğer" } },
                { "VOLTA", new List<string> { "APX 5", "EV 1", "RS 7", "VM 4", "VT 5", "Diğer" } },
                { "YAMAHA", new List<string> { "MT-07", "MT-09", "R25", "Tracer 9", "X-Max 250", "R 1", "Diğer" } },
                { "YUKI", new List<string> { "Diğer" } }
            };
        }

        // Aşağıdaki metotlar otomobil ve SUV datalarını döner
        private Dictionary<string, List<string>> GetOtomobilData()
        {
            // Buraya otomobilData dictionary'ini taşı
            return new Dictionary<string, List<string>>
            {
                { "Alfa Romeo", new List<string> { "145", "146", "147", "155", "156", "159", "166", "33", "Giulia", "Giulietta", "GT", "Mito", "Spider" } },
    { "Anadol", new List<string> { "A", "Böcek", "SV" } },
    { "Aston Martin", new List<string> { "DB11", "DB7", "Vanquish", "Vantage", "Virage" } },
    { "Audi", new List<string> { "100 Serisi", "80 Serisi", "90 Serisi", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "E-Tron GT", "R8", "RS5", "S Serisi", "TT" } },
    { "Bentley", new List<string> { "Arnage", "Bentayga", "Continental", "Flying Spur", "Mulsanne" } },
    { "BMW", new List<string> { "1 Serisi", "2 Serisi", "3 Serisi", "4 Serisi", "5 Serisi", "6 Serisi", "7 Serisi", "8 Serisi", "İ Serisi", "M Serisi", "Z Serisi" } },
    { "Cadillac", new List<string> { "BLS", "Brougham", "CTS", "DeVille", "Eldorado", "Fletwood", "Seville", "STS" } },
    { "Cherry", new List<string> { "Alia", "Chance", "Kimo", "Niche" } },
    { "Chevrolet", new List<string> { "Aveo", "Camaro", "Caprice", "Corvette", "Cruze", "Epica", "Evanda", "Kalos", "Lacetti", "Rezzo", "Spark" } },
    { "Chrysler", new List<string> { "300 C", "300 M", "Concorde", "Crossfire", "LHS", "Neon", "PT Cruiser", "Sebring", "Stratus" } },
    { "Citroen", new List<string> { "AX", "BX", "C-Elysee", "C1", "C2", "C3", "C3 Picasso", "C4", "C4 Grand Picasso", "C4 Picasso", "C5", "C6", "C8", "Evasion", "Saxo", "Xantia", "Xsara", "ZX" } },
    { "Dacia", new List<string> { "Lodgy", "Logan", "Sandero", "Solenza" } },
    { "Daewoo", new List<string> { "Espero", "Lanos", "Legenza", "Matiz", "Nexia", "Nubira", "Racer", "Tico" } },
    { "Daihatsu", new List<string> { "Applause", "Atrai", "Be-go", "Boon", "Charade", "Cuore", "Hi-Jet", "Hi-jet Kargo 660 cc", "Materia", "Mira", "Sirion", "Storia", "Taft", "Tocot", "Yrv" } },
    { "Dodge", new List<string> { "Avenger", "Challenger", "Charger", "Magnum", "Spirit", "Stealth", "Viper" } },
    { "DS Automobiles", new List<string> { "DS 3", "DS 4", "DS 4 Crossback", "Ds 5" } },
    { "Ferrari", new List<string> { "348", "360", "430", "456", "458", "488", "550", "599 GT", "612", "California", "F355", "F8", "Mondial T", "Portofino", "Roma", "SF90" } },
    { "Fiat", new List<string> { "126 Bis", "500 Abarth", "500 Ailesi", "Albea", "Brava", "Bravo", "Coupe", "Croma", "Egea", "Idea", "Linea", "Marea", "Palio", "Panda", "Punto", "Qubo", "Sedici", "Siena", "Stilo", "Tempra", "Tipo", "Uno" } },
    { "Ford", new List<string> { "B-Max", "C-Max", "Escort", "Festiva", "Fiesta", "Focus", "Fusion", "Galaxy", "Granada", "GT40", "Ka", "Mondeo", "Mustang", "Probe", "S-Max", "Scorpio", "Sierra", "Taunus", "Taurus", "Torneo Courier" } },
    { "GAZ", new List<string> { "3110" } },
    { "Geely", new List<string> { "Echo", "Emgrand", "Familia", "FC" } },
    { "Honda", new List<string> { "Accord", "Acty", "City", "Civic", "CR-Z", "CRX", "Evanda", "Fit", "Fit Aria", "Freed", "Grace", "Integra", "İnsight", "Jade", "Jazz", "Legend", "Logo", "N-Box", "N-One", "Prelude", "S2000", "S660", "Shuttle", "Stepwgn", "Stream" } },
    { "Hyundai", new List<string> { "Accent", "Accent Blue", "Accent Era", "Atos", "Coupe", "Elentra", "Excel", "Genesis", "Getz", "Ioniq", "i10", "i20", "i20 Active", "i20 Troy", "i30", "i40", "ix20", "Matrix", "S-Coupe", "Sonata", "Trajet", "Tuscani", "Veloster" } },
    { "Infiniti", new List<string> { "G", "I30", "Q30", "Q50", "Q60" } },
    { "Isuzu", new List<string> { "Gemini", "Piazza" } },
    { "Jaguar", new List<string> { "F-Type", "S-Type", "X-Type", "XE", "XF", "XJ", "XJR", "XJS", "XK8" } },
    { "Kia", new List<string> { "Capital", "Carens", "Carnival", "Ceed", "Cerato", "Clarus", "Magentis", "Opirus", "Optima", "Picanto", "Pride", "Pro Ceed", "Rio", "Sephia", "Shuma", "Stinger", "Venga" } },
    { "Lada", new List<string> { "Kalina", "Nova", "Priora", "Samara", "VAZ", "Vega" } },
    { "Lamborghini", new List<string> { "Gallardo", "Huracan" } },
    { "Lancia", new List<string> { "Delta", "Thema", "Y (Ypsilon)" } },
    { "Lexus", new List<string> { "CT 200h", "ES", "LS", "NX 300h", "RC 200t", "RC 350 F 3.5L", "UX" } },
    { "Lincoln", new List<string> { "Continental", "LS", "Mark", "MKS", "Town Car" } },
    { "Lotus", new List<string> { "Elise", "Esprit" } },
    { "Maserati", new List<string> { "Ghibli", "GranCabrio", "GranTurismo", "GT", "Levante", "Quattroporte" } },
    { "Mazda", new List<string> { "2", "3", "323", "6", "626", "929", "Atenza", "Axela", "Biante", "Carol", "Demio", "Familia", "Lantis", "MPV", "MX", "Premacy", "Roadstar", "RX 7", "RX 8", "Scrum Wagon" } },
    { "McLaren", new List<string> { "540C", "570S", "600LT", "620R", "625C", "650S", "675LT", "720S", "765LT", "Artura", "Elva", "F1", "GT" } },
    { "Mercedes-Benz", new List<string> { "115", "123", "190", "200", "220", "230", "240", "250", "260", "280", "300", "420", "500", "560", "600", "A Serisi", "AMG GT", "B Serisi", "C Serisi", "CL", "CLA", "CLC", "CLE Coupé", "CLK", "CLS", "E Serisi", "EQE", "EQS", "Maybach S", "S Serisi", "SL", "SLC", "SLK", "SLS" } },
    { "MG", new List<string> { "F-Type", "MG3", "ZR" } },
    { "MINI", new List<string> { "Cooper", "Cooper Clubman", "Cooper S", "John Cooper", "MINI Electric", "One" } },
    { "Mitsubishi", new List<string> { "Airtrek", "Attrage", "Carisma", "Cedia", "Colt", "Delica", "Dingo", "Fgalant", "İ-MİEV", "Lancer", "Mirage", "Space Star", "Space Wagon" } },
    { "Morris", new List<string> { "Marina" } },
    { "Nissan", new List<string> { "350 Z", "370 Z", "Ad", "Almera", "Altima", "Bluebird", "Cedriic", "Cube", "Datsun", "Dayz", "Figaro", "GT-R", "Latio", "Laurel Altima", "Leaf", "March", "Maxima", "Micra", "Note", "NV200", "NV350", "NC Coupe", "Pino", "Primera", "Pulsar", "Serena", "Silvia", "Skyline", "Sunny", "Sylphy", "Teana", "Tiida", "Wingroad" } },
    { "Opel", new List<string> { "Adam", "Agila", "Ascona", "Astra", "Calibra", "Cascada", "Corsa", "Insingia", "Kadett", "Meriva", "Omega", "Rekord", "Signum", "Tigra", "Vectra", "Zafira" } },
    { "Peugeot", new List<string> { "106", "107", "205", "206", "207", "208", "301", "305", "306", "307", "308", "405", "406", "407", "508", "605", "607", "806", "807", "RCZ" } },
    { "Pontiac", new List<string> {  } },
    { "Porsche", new List<string> { "718", "718 Boxster", "718 Cayman", "718 Spyder", "911", "924", "Cayenne", "Macan", "Panemera", "Taycan" } },
    { "Proton", new List<string> { "218", "315", "316", "413", "415", "416", "418", "420", "Gen 2", "Savvy", "Waja" } },
    { "Renault", new List<string> { "Clio", "Espace", "Fluence", "Grand Scenic", "Kangoo", "Laguna", "Latitude", "Lutecia", "Megane", "Megane E-Tech", "Modus", "R 11", "R12", "R 19", "R 21", "R 25", "R 5", "R 9", "Safrane", "Scenic", "Symbol", "Tailant", "Talisman", "Twingo", "Twizy", "Vel Satis", "Zoe" } },
    { "Rolls-Royce", new List<string> { "Dawn", "Ghost", "Wraith" } },
    { "Saab", new List<string> { "214", "216", "220", "25", "414", "416", "420", "45", "620", "75", "820" } },
    { "Seat", new List<string> { "Alhambra", "Altea", "Arona", "Cordoba", "Exeo", "Ibiza", "Leon", "Malaga", "Toledo" } },
    { "Skoda", new List<string> { "Citigo", "Fabia", "Favorit", "Felica", "Forman", "Octavia", "Rapid", "Roomster", "Scala", "SuperB" } },
    { "Smart", new List<string> { "ForFour", "ForTwo", "Roadster" } },
    { "Subaru", new List<string> { "BRZ", "Impreza", "Justy", "Legacy", "Levorg", "Pleo", "Vivio", "WRX STI" } },
    { "Suzuki", new List<string> { "Aerio", "Alto", "Alto Lapin", "Baleno", "Cultus", "Every", "Hustler", "Ignis", "Liana", "Maruti", "Solio", "Splash", "Swift", "SX4", "Wagon", "Wagon R" } },
    { "Tata", new List<string> {  } },
    { "Tesla", new List<string> { "Model 3", "Model S", "Model X" } },
    { "Tofaş", new List<string> {  } },
    { "Toyota", new List<string> { "Allex", "Allion", "Alphard", "Aqua", "Auris", "Avensis", "Axio", "Aygo", "bB", "Belta", "Camry", "Carina", "Celica", "Century", "Chaser", "Corolla", "Corona", "Corsa", "Cressida", "Funcargo", "GR Yaris", "GT86", "Isis", "IST", "iQ", "Mirai", "MR-S", "MR2", "Noah", "Passo", "Piicnic", "Pixis", "Platz", "Premio", "Previa", "Prius", "Probox", "Probox Hybrid", "Ractis", "Roomy", "Runx", "Scion Tc", "Sienta", "Soarer", "Sprinter", "Starlet", "Supra", "Tank", "Tercel", "Townace", "Urban Cruiser", "Vellfire", "Verossa", "Verso", "Vitz", "Voltz", "Voxy", "Yaris" } },
    { "Triumph", new List<string> { "1500", "2.5 PI MK", "2000 MKI", "2500", "Acclaim", "Dolomite", "Spitfire", "Stag", "Toledo", "TR 6", "TR 7", "TR 8" } },
    { "Vauxhall", new List<string> { "Agila", "Ampera", "Astra", "Calibra", "Carlton Mk", "Cascada", "Cavalier", "Chevette", "Corsa", "Firenza Coupe", "Insignia", "Magnum", "Nova", "Omega", "Royale", "Senator", "Signum", "Sintra", "Tigra", "Vectra", "Ventora", "Viceroy", "Victor", "Viva", "VX", "VXR8", "Zafira" } },
    { "Volkswagen", new List<string> { "Arteon", "Attrage", "Beach Buggy", "Beetle", "Bora", "Carisma", "Colt", "Corrado", "E-Golf", "EOS", "Galant", "Golf", "Id.3", "Jetta", "Lancer", "Lupo", "Passat", "Passat Variant", "Phaeton", "Polo", "Scirocco", "Sharam", "Space Star", "Space Wagon", "Touran", "Up Club", "Vento", "VW CC" } },
    { "Volvo", new List<string> { "240", "440", "460", "740", "850", "940", "960", "C30", "C70", "S40", "S60", "S70", "S80", "S90", "V40", "V40 Cross Country", "V50", "V60", "V60 Cross Country", "V60 Sports Wagon", "V70", "V90 Cross Country" } },

        };
        }

        private Dictionary<string, List<string>> GetSuvData()
        {
            // Buraya suvData dictionary'ini taşı
            return new Dictionary<string, List<string>>
            {
                { "AUDI", new List<string> { "E-Tron", "Q2", "Q3", "Q4", "Q4 Sportback", "Q5", "Q6", "Q7", "RS Q3", "RS Q8", "SQ2", "SQ5", "SQ6", "SQ7", "SQ8" } },
    { "BMW", new List<string> { "İX", "İX3", "X1", "X2", "X3", "X4", "X5", "X6", "X7", "X7M" } },
    { "CADILLAC", new List<string> { "Escalade", "SRX" } },
    { "CHERY", new List<string> { "Tiggo" } },
    { "CHEVROLET", new List<string> { "Avalanche", "Blazer", "Captiva", "Colorado", "Equinox", "HHR", "Silverado", "Suburban", "Traverse", "Trax" } },
    { "CHRYSLER", new List<string> { "Pacificia" } },
    { "CİTROEN", new List<string> { "C3 AirCross", "C4 Cactus", "C4 Suv", "C5 AirCross" } },
    { "CUPRA", new List<string> { "Formentor" } },
    { "DACIA", new List<string> { "Duster", "Sandero Stepway" } },
    { "DAIHATSU", new List<string> { "Feroza", "Rocky", "Terios" } },
    { "DODGE", new List<string> { "Caliber", "Durango", "Journey", "Nitro", "Ram" } },
    { "DS AUTOMOBILES", new List<string> { "DS3 Crossback", "DS7 Crossback" } },
    { "FIAT", new List<string> { "500 X", "Egea Cross", "Freemont", "Fullback", "Sedici" } },
    { "FORD", new List<string> { "EcoSport", "Edge", "Escape", "Expedition", "Explorer", "F Series", "Flex", "Kuga", "Maverick", "Mustang March-E", "Puma", "Ranger", "Ranger Raptor" } },
    { "GMC", new List<string> { "Canyon", "Envoy", "Jimmy", "Sierra", "Sonoma", "Yukon" } },
    { "HONDA", new List<string> { "CR-V", "Element", "HR-V", "Pilot", "Vezel", "WR-V", "ZR-V" } },
    { "HUMMER", new List<string> { "H1", "H2", "H3" } },
    { "HYUNDAI", new List<string> { "Bayon", "Galloper", "İX35", "Kona", "Santa-Fe", "Terracan", "Tuscon", "Venue" } },
    { "INFINITI", new List<string> { "EX", "FX", "QX" } },
    { "ISUZU", new List<string> { "Amigo", "Bighorn", "D-Max", "Mu", "Rodeo", "Rodeo Denver", "Trooper" } },
    { "JAGUAR", new List<string> { "E-Pace", "F-Pace", "I-Pace" } },
    { "JEEP", new List<string> { "Cherokee", "CJ", "Commander", "Compass", "Gladiator", "Grand Chrokee", "Patriot", "Renegade", "Wrangler" } },
    { "KIA", new List<string> { "EV9", "Niro", "Retona", "Sorento", "Soul", "Sportage", "Stonic", "Xceed" } },
    { "LAMBORGHINI", new List<string> { "Urus" } },
    { "LAND ROVER", new List<string> { "Defender", "Discovery", "Discovery Sport", "Freelander", "Range Rover", "Range Rover Evoque", "Range Rover Sport", "Range Rover Velar" } },
    { "LEXUS", new List<string> { "LX 600", "NX", "RX", "RX L" } },
    { "LINCOLN", new List<string> { "Aviator", "MKX", "Nautilus", "Navigator" } },
    { "MASERATI", new List<string> { "Grecale", "Levante" } },
    { "MAZDA", new List<string> { "B Serisi", "CX-3", "CX-30", "CX-5", "CX-60", "CX-8", "CX-9", "MX-30", "Tribute" } },
    { "MERCEDES-BENZ", new List<string> { "EQA", "EQB", "EQC", "EQE", "G Serisi", "GL", "GLA", "GLB", "GLC", "GLC Coupe", "GLE", "GLE Coupe", "GLK", "GLS", "ML", "X" } },
    { "MG", new List<string> { "HS", "ZS", "ZS EV", "ZST" } },
    { "MINI COOPER", new List<string> { "Cooper Countryman" } },
    { "MITSUBISHI", new List<string> { "ASX", "Eclipse Cross", "L200", "Outlander", "Pajero", "RVR", "Strada", "Triton" } },
    { "NISSAN", new List<string> { "Country", "Datsun", "Dualis", "Juke", "Kicks", "Murano", "Navara", "Pathfinder", "Patrol", "Qashqai", "Skystar", "Terrano", "X-Trail" } },
    { "OPEL", new List<string> { "Antara", "Crossland", "Crossland X", "Frontera", "Grandland X", "Mokka", "Mokka E", "Mokka X", "Monterey" } },
    { "PEUGEOT", new List<string> { "2008", "3008", "4007", "5008", "E-2008" } },
    { "PORSCHE", new List<string> { "Cayenne", "Cayenne Coupe", "Macan" } },
    { "RENAULT", new List<string> { "Captur", "Kadjar", "Koleos", "Scenic RX-4" } },
    { "SEAT", new List<string> { "Arona", "Ateca", "Tarraco" } },
    { "SKODA", new List<string> { "Kamiq", "Karoq", "Kodiaq", "Yeti" } },
    { "SSANGYONG", new List<string> { "Acyton", "Acyton Sport", "Korando", "Korando Sport", "Kyron", "Musso", "Musso Grand", "Rexton", "Rodius", "Tivoli", "XLV" } },
    { "SUBARU", new List<string> { "Forester", "Outback", "Tribeca", "XV" } },
    { "SUZUKI", new List<string> { "Escudo", "Grand Vitara", "Jimmy", "Samurai", "SJ", "SX4 S-Cross", "Vitara", "X-90" } },
    { "TATA", new List<string> { "Safari", "Telcoline", "Xenon" } },
    { "TESLA", new List<string> { "Model Y" } },
    { "TOYOTA", new List<string> { "4Runner", "Aqua Cross", "C-HR", "Cami", "Corolla", "Crown Sport", "FJ Cruiser", "Fortuner", "Highlander", "Hilux", "Land Curuiser", "Prado", "Raize", "RAV4", "Rush", "Tundra", "Venza", "Yaris Cross" } },
    { "VOLKSWAGEN", new List<string> { "Amarok", "ID.3", "ID4", "T-Cross", "T-Roc", "Taigo", "Tiguan", "Tiguan AllSpace", "Toureg" } },
    { "VOLVO", new List<string> { "EX30", "XC40", "XC60", "XC70", "XC90" } },
            };
        }
    }
}
