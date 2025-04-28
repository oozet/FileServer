import React, { useState } from "react";
import { useAuth } from "../../context/auth-context";

const FileUpload: React.FC<{ activeDirectory: { name: string; id: number | string } | null }> = ({ activeDirectory }) => {

    const [selectedFiles, setSelectedFiles] = useState<File[]>([]);

    const SetFile = (file: File) => {
        setSelectedFiles((prevFiles) => [...prevFiles, file]);
    };

    const [message, setMessage] = useState<string>("");
    const { accessToken } = useAuth();

    // Handle file input change
    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        if (event.target.files && event.target.files.length > 0) {
            const oversizedFileList: string[] = [];

            const maxSize = 10 * 1024 * 1024;
            for (const file of event.target.files) {
                if (file.size > maxSize) {
                    oversizedFileList.push(file.name);
                    continue;
                }

                SetFile(file);
            }

            if (oversizedFileList.length > 0) {
                alert(`The following files exceed the size limit:\n${oversizedFileList.join("\n")}`);
            }
        }
    };

    // Handle file upload
    const handleFileUpload = async (event: React.FormEvent) => {
        event.preventDefault();
        if (!selectedFiles || !activeDirectory) {
            setMessage("Please select a file and directory before uploading.");
            return;
        }

        const id = activeDirectory?.id?.toString();
        if (!id) {
            setMessage("Directory id is missing. Select a valid directory.");
            return;
        }
        console.log("Directory id: " + id);

        const formData = new FormData();

        for (const file of selectedFiles) {
            formData.append("files", file); // string must match endpoint parameter
        }
        formData.append("directoryId", id);

        for (const [key, value] of formData.entries()) {
            console.log(`${key}: ${value}`);
        }

        try {
            const response = await fetch("http://localhost:5264/storage/", {
                method: "POST",
                headers: {
                    "Authorization": `Bearer ${accessToken}`,
                },
                body: formData,
            });

            if (response.ok) {
                window.location.reload();
            } else {
                const errorText = await response.text();
                setMessage(`File upload failed: ${errorText}`);
            }
        } catch (error) {
            setMessage(`Error occurred: ${error}`);
        }
    };

    return (
        <div>
            <form onSubmit={handleFileUpload}>
                <input type="file" onChange={handleFileChange} multiple />
                <button type="submit" disabled={!activeDirectory}>
                    {activeDirectory ? (<span>Upload to {activeDirectory.name}</span>) : (<span>Select directory to upload</span>)}
                </button>
                {message && <p>{message}</p>}
            </form>
        </div>
    );
};

export default FileUpload;

