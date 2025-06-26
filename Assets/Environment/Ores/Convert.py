import yaml
import re

def extract_physics_shapes(content):
    """Extract all physicsShape entries from YAML content."""
    physics_shapes = []
    pattern = re.compile(r'physicsShape:\s*\n(?:\s*-\s*-\s*{x:.*?}\n?)+', re.MULTILINE)

    for match in pattern.finditer(content):
        yaml_block = match.group(0)
        parsed = yaml.safe_load(yaml_block)
        physics_shapes.append(parsed['physicsShape'])

    return physics_shapes

def replace_empty_physics_shapes(b_content, shapes):
    """Replace empty physicsShape: [] entries in B with those from A."""
    lines = b_content.splitlines()
    new_lines = []
    shape_index = 0
    i = 0

    while i < len(lines):
        line = lines[i]
        if re.match(r'\s*physicsShape:\s*\[\s*\]\s*', line) and shape_index < len(shapes):
            # Get indentation
            indent = re.match(r'^(\s*)', line).group(1)
            # Build new physicsShape block
            shape_lines = [f"{indent}physicsShape:"]
            for sublist in shapes[shape_index]:
                shape_lines.append(f"{indent}  - " + yaml.dump([sublist], default_flow_style=True).strip())
            new_lines.extend(shape_lines)
            shape_index += 1
        else:
            new_lines.append(line)
        i += 1

    return '\n'.join(new_lines)

def main():
    with open('file_a.yaml', 'r') as fa:
        a_content = fa.read()

    with open('file_b.yaml', 'r') as fb:
        b_content = fb.read()

    # Extract shapes from file A
    shapes = extract_physics_shapes(a_content)

    # Replace placeholders in file B
    updated_b = replace_empty_physics_shapes(b_content, shapes)

    # Write back to a new file or overwrite
    with open('file_b_updated.yaml', 'w') as fout:
        fout.write(updated_b)

if __name__ == "__main__":
    main()