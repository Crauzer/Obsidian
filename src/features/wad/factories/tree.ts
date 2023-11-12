import { VirtualizedWadTreeItem, WadItem } from '../types';

export const flattenWadTree = (items: WadItem[]): VirtualizedWadTreeItem[] => {
  const traverse_items = (items: WadItem[], level: number) => {
    return items.reduce<VirtualizedWadTreeItem[]>((acc, current, _index) => {
      acc.push({
        kind: current.kind,
        level,
        id: current.id,
        path: current.path,
        name: current.name,
        nameHash: current.nameHash,
        pathHash: current.pathHash,
        isSelected: current.isSelected,
        isChecked: current.isChecked,
        isExpanded: current.kind === 'directory' ? current.isExpanded : false,
      });

      if (current.kind === 'directory' && current.isExpanded) {
        acc = [...acc, ...traverse_items(current.items, level + 1)];
      }

      return acc;
    }, []);
  };

  return traverse_items(items, 0);
};
