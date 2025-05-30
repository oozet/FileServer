import React, { useState } from "react";
import TreeBuilder from "./tree-builder";
import FileUpload from "../files/file-upload";


const DirectoryComponent: React.FC = () => {
    const [activeDirectory, setActiveDirectory] = useState<{ name: string; id: number | string } | null>(null);

    console.log("Active Directory:", activeDirectory);

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
