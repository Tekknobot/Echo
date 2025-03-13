from PIL import Image, ImageDraw
import os

# Image dimensions.
width, height = 512, 512

# Create a new RGBA image (transparent background).
img = Image.new("RGBA", (width, height), (0, 0, 0, 0))
draw = ImageDraw.Draw(img)

# Define the face circle.
face_center = (width // 2, height // 2)
face_radius = int(width * 0.45)
face_bbox = [
    face_center[0] - face_radius, face_center[1] - face_radius,
    face_center[0] + face_radius, face_center[1] + face_radius
]

# Draw the face: yellow fill with a black outline.
draw.ellipse(face_bbox, fill=(255, 255, 0, 255), outline=(0, 0, 0, 255), width=4)

# Draw the eyes: two small black circles.
eye_radius = int(face_radius * 0.15)
# Left eye.
left_eye_center = (face_center[0] - face_radius // 2, face_center[1] - face_radius // 3)
left_eye_bbox = [
    left_eye_center[0] - eye_radius, left_eye_center[1] - eye_radius,
    left_eye_center[0] + eye_radius, left_eye_center[1] + eye_radius
]
draw.ellipse(left_eye_bbox, fill=(0, 0, 0, 255))
# Right eye.
right_eye_center = (face_center[0] + face_radius // 2, face_center[1] - face_radius // 3)
right_eye_bbox = [
    right_eye_center[0] - eye_radius, right_eye_center[1] - eye_radius,
    right_eye_center[0] + eye_radius, right_eye_center[1] + eye_radius
]
draw.ellipse(right_eye_bbox, fill=(0, 0, 0, 255))

# Draw the smiling mouth as an arc.
# Define a bounding box for the arc.
mouth_bbox = [
    face_center[0] - int(face_radius * 0.75),
    face_center[1] - int(face_radius * 0.2),
    face_center[0] + int(face_radius * 0.75),
    face_center[1] + int(face_radius * 0.75)
]
draw.arc(mouth_bbox, start=20, end=160, fill=(0, 0, 0, 255), width=4)

# Save the image to the Desktop.
desktop_path = os.path.join(os.path.expanduser("~"), "Desktop")
save_path = os.path.join(desktop_path, "happy_face_texture.png")
img.save(save_path)
print("Happy face texture saved to:", save_path)

# Optionally display the image.
img.show()
