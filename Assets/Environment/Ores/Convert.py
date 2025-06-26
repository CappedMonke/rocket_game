import sys
from ruamel.yaml import YAML

def main():
    if len(sys.argv) != 3:
        print("Usage: python script.py <fileA> <fileB>")
        sys.exit(1)
    
    fileA_path = sys.argv[1]
    fileB_path = sys.argv[2]
    
    yaml = YAML()
    yaml.preserve_quotes = True
    
    # Load and collect non-empty physicsShapes from fileA
    with open(fileA_path, 'r') as f:
        dataA = yaml.load(f)
    
    physics_shapes = []
    
    def collect_physics_shapes(data):
        if isinstance(data, dict):
            for key, value in data.items():
                if key == "physicsShape":
                    if isinstance(value, list) and value:
                        physics_shapes.append(value)
                else:
                    collect_physics_shapes(value)
        elif isinstance(data, list):
            for item in data:
                collect_physics_shapes(item)
    
    collect_physics_shapes(dataA)
    
    # Load fileB and replace empty physicsShapes
    with open(fileB_path, 'r') as f:
        dataB = yaml.load(f)
    
    idx = 0
    
    def replace_physics_shapes(data):
        nonlocal idx
        if isinstance(data, dict):
            for key, value in list(data.items()):
                if key == "physicsShape":
                    if isinstance(value, list) and value == []:
                        if idx < len(physics_shapes):
                            data[key] = physics_shapes[idx]
                            idx += 1
                else:
                    replace_physics_shapes(value)
        elif isinstance(data, list):
            for item in data:
                replace_physics_shapes(item)
    
    replace_physics_shapes(dataB)
    
    # Save the modified data back to fileB
    with open(fileB_path, 'w') as f:
        yaml.dump(dataB, f)

if __name__ == "__main__":
    main()