from PIL import Image, ImageDraw
import os

# Define the tile size and grid dimensions.
tileSize = 64
numTiles = 8
size = tileSize * numTiles

# Create a new image with RGB mode.
img = Image.new("RGB", (size, size), "white")
draw = ImageDraw.Draw(img)

# Define two colors for the checkered pattern.
color1 = (180, 180, 180)  # Light gray
color2 = (100, 100, 100)  # Dark gray

# Draw the checkered pattern.
for y in range(numTiles):
    for x in range(numTiles):
        color = color1 if (x + y) % 2 == 0 else color2
        topLeft = (x * tileSize, y * tileSize)
        bottomRight = ((x + 1) * tileSize, (y + 1) * tileSize)
        draw.rectangle([topLeft, bottomRight], fill=color)

# Construct the path to the Desktop.
desktop_path = os.path.join(os.path.expanduser("~"), "Desktop")
save_path = os.path.join(desktop_path, "floor_texture.png")

# Save the image to the Desktop.
img.save(save_path)
print(f"Image saved to: {save_path}")

# Optionally display the image using the default image viewer.
img.show()
