import React, { useState } from "react";
import TreeBuilder, { TreeNode } from "./tree-builder";
import FileUpload from "../files/file-upload";


const DirectoryComponent: React.FC = () => {
    const [activeDirectory, setActiveDirectory] = useState<{ name: string; id: number | string } | null>(null);

    console.log("Active Directory:", activeDirectory);

    const downloadFile = async (file: TreeNode) => {
        try {
            const response = await fetch(`/api/files/download/${file.name}`);
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

    return (
        <div>
            <h1>Directory Tree</h1>
            <TreeBuilder activeDirectory={activeDirectory} setActiveDirectory={setActiveDirectory} />
            <p>Active Directory: {activeDirectory?.name || "None"}</p>

            <FileUpload activeDirectory={activeDirectory} />
        </div>
    );
};

export default DirectoryComponent;
