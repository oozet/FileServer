import React, { useEffect, useState } from "react";
import { useAuth } from "../../context/auth-context";
import DirectoryTree from "./directory-tree";


export interface TreeNode {
    id: number | string;
    name: string;
    parentDirectoryId: number | null;
    type: "directory" | "file";
    children: TreeNode[]; // Only for directories
    collapsed?: boolean;
}

interface Directory {
    id: number;
    name: string;
    parentDirectoryId: number | null;
}

interface FileDto {
    id: string;
    name: string;
    parentDirectoryId: number;
}

function buildTree(directories: Directory[], files: FileDto[]): TreeNode[] {
    const nodeMap: Record<string, TreeNode> = {};

    // Add directories to the map
    directories.forEach((dir) => {
        nodeMap[dir.id] = {
            ...dir,
            type: "directory",
            children: [],
            collapsed: true
        };
    });


    // Add files to the map (files don’t have children)
    if (files.length > 0) {
        files.forEach((file) => {
            nodeMap[file.id] = {
                id: file.id,
                name: file.name,
                parentDirectoryId: file.parentDirectoryId,
                type: "file",
                children: [], // Empty because files can't have children
            };
        });
    }

    const tree: TreeNode[] = [];

    // Build the hierarchical structure
    directories.forEach((dir) => {
        if (dir.parentDirectoryId === null) {
            // Root directory
            tree.push(nodeMap[dir.id]);
        } else {
            // Add directory as a child of its parent
            nodeMap[dir.parentDirectoryId]?.children.push(nodeMap[dir.id]);
        }
    });

    files.forEach((file) => {
        // Add file as a child of its parent directory
        nodeMap[file.parentDirectoryId]?.children.push(nodeMap[file.id]);
    });

    return tree;
}

// const DirectoryTree: React.FC<DirectoryTreeProps> = ({ nodes, onFileClick, activeDirectory, setActiveDirectory }) => {

type TreeBuilderProps = {
    activeDirectory: { name: string; id: number | string } | null;
    setActiveDirectory: (directory: { name: string, id: number | string } | null) => void
}

const TreeBuilder: React.FC<TreeBuilderProps> = ({ activeDirectory, setActiveDirectory }) => {
    const [tree, setTree] = useState<TreeNode[]>([]);
    const [newDirectoryName, setNewDirectoryName] = useState<string | null>(null);

    const { accessToken } = useAuth();

    useEffect(() => {
        fetchStorageTree();
    }, []);

    const fetchStorageTree = async () => {
        fetch("http://localhost:5264/storage/", {
            method: 'GET',
            credentials: 'include', // Include cookies for authentication
            headers: {
                Authorization: `Bearer ${accessToken}` // Include your access token in the headers
            }
        })
            .then((res) => res.json())
            .then((data: { directories: Directory[]; files: FileDto[] }) => {
                const { directories, files } = data;
                setTree(buildTree(directories, files));
            })
            .catch((error) => console.error("Fetch failed:", error));
    }

    const createDirectory = async () => {
        if (!activeDirectory?.id) {
            alert("Need to select a directory first.");
            return;
        } else if (!newDirectoryName) {
            alert("The name cannot be empty.");
            return;
        }

        const requestBody = {
            name: newDirectoryName,
            parentDirectoryId: activeDirectory.id,
        };

        try {
            const response = await fetch("http://localhost:5264/storage/create-dir", {
                method: "POST",
                credentials: "include", // Include cookies for authentication
                headers: {
                    Authorization: `Bearer ${accessToken}`,
                    "Content-Type": "application/json", // Ensure JSON is correctly parsed
                },
                body: JSON.stringify(requestBody),
            });
            if (!response.ok) throw new Error("Failed to create directory");
            await fetchStorageTree();
        } catch (error) {
            console.error("Error creating directory:", error);
        }
    };



    const downloadFile = async (file: TreeNode) => {
        try {
            const response = await fetch(`http://localhost:5264/storage/download/${file.id}`, {
                method: "POST",
                credentials: "include", // Include cookies for authentication
                headers: {
                    Authorization: `Bearer ${accessToken}`,
                    "Content-Type": "application/json", // Ensure JSON is correctly parsed
                },
            });

            if (!response.ok) {
                throw new Error("Failed to download file");
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);

            const link = document.createElement("a");
            link.href = url;
            link.download = file.name;
            link.click();

            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error("Error downloading file:", error);
        }
    };

    return (<>
        <DirectoryTree nodes={tree} onFileClick={downloadFile} activeDirectory={activeDirectory} setActiveDirectory={setActiveDirectory} />
        <label htmlFor="directoryName">New directory name:</label>
        <input name="directoryName" value={newDirectoryName ?? ""} onChange={(e) => setNewDirectoryName(e.target.value)} />
        <button onClick={createDirectory}>Create Directory</button>
    </>);
};

export default TreeBuilder;
