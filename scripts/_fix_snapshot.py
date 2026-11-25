import os
import re
import sys

SRC = r"src\CafePOSManagement.Web\CafePOSManagement\Web\Data\Migrations\CafePosDbContextModelSnapshot.cs"
DST = r"src\CafePOSManagement.Web\Data\Migrations\CafePosDbContextModelSnapshot.cs"
DESIGNER = r"src\CafePOSManagement.Web\Data\Migrations\20261118010000_AddKhaMenuAndUniqueImages.Designer.cs"
STRAY_DIR = r"src\CafePOSManagement.Web\CafePOSManagement"

CREATED = ("new DateTimeOffset(new DateTime(2026, 11, 7, 8, 0, 0, 0, DateTimeKind.Unspecified), "
           "new TimeSpan(0, 7, 0, 0, 0))")
BASE = "https://images.unsplash.com/photo-{}?auto=format&fit=crop&w=800&q=85"
NL = "\r\n"

# id, category, name, originalPrice, price, photo-id
PRODUCTS = [
    ("p1", "traditional", "Cà phê sữa đá", "55000m", "45000m", "1461023058943-07fcbe16d735"),
    ("p2", "espresso", "Cappuccino", "65000m", "55000m", "1572442388796-11668a67e53d"),
    ("p3", "snack", "Bánh Croissant bơ", "50000m", "45000m", "1555507036-ab1f4038808a"),
    ("p4", "espresso", "Americano nóng", "45000m", "40000m", "1514432324607-a09d9b4aefdd"),
    ("p5", "traditional", "Bạc xỉu đá", None, "39000m", "1517701604599-bb29b565090c"),
    ("p6", "sandwich", "Sandwich nướng", "75000m", "65000m", "1528735602780-2552fd46c7af"),
    ("p7", "coldbrew", "Cold Brew sữa tươi", "60000m", "55000m", "1517256064527-09c73fc73e38"),
    ("p8", "tea", "Trà chanh thanh mát", None, "45000m", "1556679343-c7306c1976bc"),
    ("p9", "espresso", "Latte kem sữa", "65000m", "59000m", "1509042239860-f550ce710b93"),
    ("p10", "seasonal", "Cà phê sữa mùa đông", None, "49000m", "1512568400610-62da28bc8a13"),
    ("p11", "coldbrew", "Cold Brew đá", "55000m", "49000m", "1517701550927-30cf4ba1dba5"),
    ("p12", "snack", "Bánh quy bơ", "35000m", "29000m", "1499636136210-6f4ee915583e"),
    ("p13", "seasonal", "Cà phê bọt biển Whipped Coffee", "65000m", "45000m", "1589396575653-c09c794ff6a6"),
    ("p14", "traditional", "Cà phê phin Filter Coffee", None, "22000m", "1511920170033-f8396924c348"),
    ("p15", "espresso", "Latte nghệ thuật", None, "60000m", "1534778101976-62847782c213"),
    ("p16", "coldbrew", "Cold Brew cam sả", None, "45000m", "1544787219-7f47ccb76574"),
    ("p17", "matcha", "Matcha Latte", "65000m", "55000m", "1536256263959-770b48d82b0a"),
    ("p18", "matcha", "Matcha Freeze", "69000m", "59000m", "1515823064-d6e0c04616a7"),
    ("p19", "matcha", "Cacao sữa", None, "49000m", "1542990253-0d0f5be5f0ed"),
    ("p20", "milktea", "Trà sữa Oolong nướng", "55000m", "48000m", "1525385133512-2f3bdd039054"),
    ("p21", "milktea", "Hồng trà Macchiato", "52000m", "45000m", "1576092768241-dec231879fc3"),
    ("p22", "milktea", "Trà sữa lài", None, "42000m", "1579954115545-a95591f28bfc"),
    ("p23", "combo", "Combo Sáng: Cà phê + Bánh croissant", "95000m", "75000m", "1525610553991-2bede1a236e2"),
    ("p24", "combo", "Combo Trà chiều: Trà đào + Bánh", "85000m", "69000m", "1509440159596-0249088772ff"),
]

CATEGORIES = [
    ("matcha", "Matcha & Đá xay", 8),
    ("milktea", "Trà sữa", 9),
    ("combo", "Combo Tiết Kiệm (Deal Hời)", 10),
]


def url(photo_id):
    return BASE.format(photo_id)


def read(path):
    with open(path, "r", encoding="utf-8-sig", newline="") as f:
        return f.read()


def write(path, text):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8-sig", newline="") as f:
        f.write(text)


def product_block(p):
    pid, cat, name, orig, price, photo = p
    desc = f"{name} — món đặc sắc của CafePOS."
    lines = [
        "                        new",
        "                        {",
        f'                            Id = "{pid}",',
        f'                            CategoryId = "{cat}",',
        f"                            CreatedAt = {CREATED},",
        f'                            Description = "{desc}",',
        f'                            ImageUrl = "{url(photo)}",',
        "                            IsAvailable = true,",
        f'                            Name = "{name}",',
    ]
    if orig is not None:
        lines.append(f"                            OriginalPrice = {orig},")
    lines.append(f"                            Price = {price}")
    lines.append("                        }")
    return NL.join(lines)


def category_block(c):
    cid, name, order = c
    lines = [
        "                        new",
        "                        {",
        f'                            Id = "{cid}",',
        f'                            Code = "{cid}",',
        f"                            CreatedAt = {CREATED},",
        f"                            DisplayOrder = {order},",
        "                            IsActive = true,",
        f'                            Name = "{name}"',
        "                        }",
    ]
    return NL.join(lines)


def append_seed(text, marker, blocks):
    """Append HasData items right before the closing of the HasData(...) call preceding `marker`."""
    idx = text.index(marker)
    close = "                        });"
    j = text.rindex(close, 0, idx)
    joined = ("," + NL).join(blocks)
    # `close` already contains the final item's closing brace, so the last block must
    # not carry its own; append only `);` after the joined blocks.
    replacement = "                        }," + NL + joined + ");"
    return text[:j] + replacement + text[j + len(close):]


text = read(SRC)
assert "OriginalPrice = null" not in text, "unexpected: snapshot encodes null OriginalPrice"

# 1. Refresh Description/ImageUrl for the 16 original products (block-scoped, one match each).
for pid, _cat, name, _orig, _price, photo in PRODUCTS[:16]:
    desc = f"{name} — món đặc sắc của CafePOS."
    pattern = re.compile(
        r'(Id = "' + re.escape(pid) + r'",(?:(?!Id = "p).)*?Description = ")[^"]*'
        r'(",' + r"\s*" + r'ImageUrl = ")[^"]*(")',
        re.S,
    )
    text, count = pattern.subn(
        lambda m: m.group(1) + desc + m.group(2) + url(photo) + m.group(3), text
    )
    if count != 1:
        sys.exit(f"expected exactly 1 rewrite for {pid}, got {count}")

# 2. Append 8 new Non-Coffee products to the Products seed.
text = append_seed(
    text,
    NL + '                });' + NL + NL + '            modelBuilder.Entity("CafePOSManagement.Web.Models.Entities.User"',
    [product_block(p) for p in PRODUCTS[16:]],
)

# 3. Append 3 new categories to the Categories seed.
text = append_seed(
    text,
    NL + '                });' + NL + NL + '            modelBuilder.Entity("CafePOSManagement.Web.Models.Entities.Discount"',
    [category_block(c) for c in CATEGORIES],
)

write(DST, text)

# 4. Designer for the new migration = same target model, different class/attribute.
designer = text
designer = designer.replace(
    "using Microsoft.EntityFrameworkCore.Metadata;",
    "using Microsoft.EntityFrameworkCore.Metadata;" + NL + "using Microsoft.EntityFrameworkCore.Migrations;",
    1,
)
before = ("    [DbContext(typeof(CafePosDbContext))]" + NL +
          "    partial class CafePosDbContextModelSnapshot : ModelSnapshot" + NL +
          "    {" + NL +
          "        protected override void BuildModel(ModelBuilder modelBuilder)")
after = ("    [DbContext(typeof(CafePosDbContext))]" + NL +
         '    [Migration("20261118010000_AddKhaMenuAndUniqueImages")]' + NL +
         "    partial class AddKhaMenuAndUniqueImages : Migration" + NL +
         "    {" + NL +
         "        /// <inheritdoc />" + NL +
         "        protected override void BuildTargetModel(ModelBuilder modelBuilder)")
if before not in designer:
    sys.exit("snapshot header not found")
designer = designer.replace(before, after, 1)
write(DESIGNER, designer)

# 5. Remove the stray duplicated folder created by an earlier bad command.
import shutil
if os.path.isdir(STRAY_DIR):
    shutil.rmtree(STRAY_DIR)

print("snapshot lines:", text.count(NL))
print("products:", len(PRODUCTS), "categories:", len(CATEGORIES))
print("unique photo ids:", len({p[5] for p in PRODUCTS}))
