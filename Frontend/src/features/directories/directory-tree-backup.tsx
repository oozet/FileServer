// import { useState } from "react";

// export type Node = {
//     name: string;
//     type: "directory" | "file";
//     children?: Node[];
//     collapsed?: boolean;
// };

// type DirectoryTreeProps = {
//     nodes: Node[];
//     onFileClick: (file: Node) => void;
//     activeDirectory: string | null;
//     setActiveDirectory: (directory: string) => void
// };

// const DirectoryTree: React.FC<DirectoryTreeProps> = ({ nodes, onFileClick, activeDirectory, setActiveDirectory }) => {
//     const [tree, setTree] = useState<Node[]>(nodes);

//     const onNodeClick = (node: Node) => {

//         if (node.type === 'directory') {
//             node.collapsed = !node.collapsed;
//             setTree([...tree]);
//         }
//         else if (node.type === 'file') {
//             onFileClick(node);
//         }
//     }

//     return (
//         <ul>
//             {tree.map((node, index) => (
//                 <li key={index}>
//                     <span onClick={() => onNodeClick(node)} style={{ cursor: "pointer" }}>
//                         {node.type === "directory"
//                             ? node.collapsed
//                                 ? "📁▶ "
//                                 : "📁▼ "
//                             : "📄 "}
//                         {node.name}
//                         {node.type === "file" ? "💾" : ""}
//                     </span>
//                     {node.type === "directory" && (
//                         <span
//                             onClick={(event) => {
//                                 event.stopPropagation();
//                                 event.preventDefault(); // Suppress any default actions
//                                 setActiveDirectory(node.name); // Update state
//                                 console.log(`Active directory set to: ${node.name}`);
//                             }}
//                             style={{
//                                 marginLeft: "8px",
//                                 cursor: "pointer",
//                                 background: "none",
//                                 border: "none",
//                             }}
//                         >
//                             {activeDirectory === node.name ? "☑" : "☐"} {/* Symbol ☐☑ Replace with a relevant icon (e.g., folder icon for uploads) */}
//                         </span>
//                     )}
//                     {!node.collapsed && node.children && (
//                         <DirectoryTree
//                             nodes={node.children}
//                             onFileClick={onFileClick}
//                             activeDirectory={activeDirectory}
//                             setActiveDirectory={setActiveDirectory}
//                         />
//                     )}
//                 </li>
//             ))}
//         </ul>
//     );
// };

// // const DirectoryTree: React.FC<DirectoryTreeProps> = ({
// //     nodes,
// //     activeDirectory,
// //     setActiveDirectory,
// //     onFileClick,
// // }) => {
// //     const [tree, setTree] = useState<Node[]>(nodes);

// //     const toggleCollapse = (node: Node) => {
// //         if (node.type === "directory") {
// //             node.collapsed = !node.collapsed;
// //             setTree([...tree]);
// //         }
// //     };

// //     return (
// //         <ul>
// //             {tree.map((node, index) => (
// //                 <li key={index}>
// //                     <span
// //                         onClick={() => toggleCollapse(node)}
// //                         style={{ cursor: "pointer" }}
// //                     >
// //                         {node.type === "directory" ? (node.collapsed ? "▶️ " : "▼ ") : "📄 "}
// //                         {node.name}
// //                     </span>
// //                     {node.type === "directory" && (
// //                         <span
// //                             onClick={() => setActiveDirectory(node.name)}
// //                             style={{ cursor: "pointer", marginLeft: "8px" }}
// //                         >
// //                             {activeDirectory === node.name ? "☑" : "☐"}
// //                         </span>
// //                     )}
// //                     {!node.collapsed && node.children && (
// //                         <DirectoryTree
// //                             nodes={node.children}
// //                             activeDirectory={activeDirectory}
// //                             setActiveDirectory={setActiveDirectory}
// //                             onFileClick={onFileClick}
// //                         />
// //                     )}
// //                 </li>
// //             ))}
// //         </ul>
// //     );
// // };


// export default DirectoryTree;