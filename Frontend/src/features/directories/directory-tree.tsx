import { useState } from "react";
import { TreeNode } from "./tree-builder";

type DirectoryTreeProps = {
    nodes: TreeNode[];
    onFileClick: (file: TreeNode) => void;
    activeDirectory: { name: string; id: number | string } | null;
    setActiveDirectory: (directory: { name: string, id: number | string } | null) => void
};

const DirectoryTree: React.FC<DirectoryTreeProps> = ({ nodes, onFileClick, activeDirectory, setActiveDirectory }) => {
    const [tree, setTree] = useState<TreeNode[]>(nodes);

    const onNodeClick = (node: TreeNode) => {

        if (node.type === 'directory') {
            node.collapsed = !node.collapsed;
            setTree([...tree]);
        }
        else if (node.type === 'file') {
            console.log("file clicked");
            onFileClick(node);
        }
    }

    return (
        <ul>
            {nodes.map((node, index) => (
                <li key={index}>
                    <span onClick={() => onNodeClick(node)} style={{ cursor: "pointer" }}>
                        {node.type === "directory"
                            ? node.collapsed
                                ? "📁▶ "
                                : "📁▼ "
                            : "📄 "}
                        {node.name}
                        {node.type === "file" ? "💾" : ""}
                    </span>
                    {node.type === "directory" && (
                        <span
                            onClick={() => {
                                //onClick={(event) => {
                                // event.stopPropagation();
                                // event.preventDefault(); // Suppress any default actions
                                setActiveDirectory({ name: node.name, id: node.id }); // Update state
                                console.log(`Active directory set to: ${node.name}`);
                            }}
                            style={{
                                marginLeft: "8px",
                                cursor: "pointer",
                                background: "none",
                                border: "none",
                            }}
                        >
                            {activeDirectory?.name === node.name ? "☑" : "☐"} {/* Symbol ☐☑ Replace with a relevant icon (e.g., folder icon for uploads) */}
                        </span>
                    )}
                    {!node.collapsed && node.children && (
                        <DirectoryTree
                            nodes={node.children}
                            onFileClick={onFileClick}
                            activeDirectory={activeDirectory}
                            setActiveDirectory={setActiveDirectory}
                        />
                    )}
                </li>
            ))}
        </ul>
    );
};

export default DirectoryTree;