from PIL import Image, ImageDraw, ImageFilter
import random
import os

# Define the image dimensions.
width, height = 512, 512

# Base cement color (a light gray).
base_color = (210, 210, 210)
img = Image.new("RGB", (width, height), base_color)
draw = ImageDraw.Draw(img)

# Add random noise for texture.
noise_intensity = 50  # How much the noise can vary from the base color.
num_points = 20000     # Number of noise points.

for _ in range(num_points):
    x = random.randint(0, width - 1)
    y = random.randint(0, height - 1)
    # Create a slight variation in color.
    delta = random.randint(-noise_intensity, noise_intensity)
    r = min(max(base_color[0] + delta, 0), 255)
    g = min(max(base_color[1] + delta, 0), 255)
    b = min(max(base_color[2] + delta, 0), 255)
    draw.point((x, y), fill=(r, g, b))

# Add some subtle crack lines to enhance the cement feel.
crack_color = (100, 100, 100)  # Darker gray for cracks.
num_cracks = 0

for _ in range(num_cracks):
    # Start at a random position.
    x0 = random.randint(0, width - 1)
    y0 = random.randint(0, height - 1)
    points = [(x0, y0)]
    # Create a meandering line.
    length = random.randint(20, 100)
    current_x, current_y = x0, y0
    for _ in range(length):
        # Small random steps.
        current_x += random.randint(-2, 2)
        current_y += random.randint(-2, 2)
        # Keep points within image boundaries.
        current_x = max(0, min(current_x, width - 1))
        current_y = max(0, min(current_y, height - 1))
        points.append((current_x, current_y))
    draw.line(points, fill=crack_color, width=1)

# Apply a slight Gaussian blur to blend the noise and cracks subtly.
img = img.filter(ImageFilter.GaussianBlur(radius=0.5))

# Save the image to the Desktop.
desktop_path = os.path.join(os.path.expanduser("~"), "Desktop")
save_path = os.path.join(desktop_path, "cement_texture.png")
img.save(save_path)

print("Cement texture saved to:", save_path)
img.show()
