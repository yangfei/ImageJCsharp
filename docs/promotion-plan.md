# Promotion Plan

This document describes how to help more people discover and join ImageJCsharp.

The goal is not only to get stars. The goal is to attract useful feedback, contributors, testers, and users who understand ImageJ workflows.

## Project Message

Short message:

> ImageJCsharp is a C# native ImageJ-like scientific image analysis application for Windows.

Longer message:

> ImageJCsharp is an open-source effort to build a C#/.NET replacement for common ImageJ workflows on Windows. The project starts with practical MVP features: opening images, viewing, ROI selection, measurements, and basic processing. The long-term goal is a C# plugin ecosystem for scientific image analysis.

## Target Audiences

### C# and .NET Developers

What they may care about:

- WinForms desktop application.
- Image processing algorithms.
- Plugin architecture.
- Open-source contribution opportunities.

Where to reach them:

- GitHub topics.
- Reddit `r/dotnet`.
- C# Discord or forums.
- Chinese .NET communities.
- Blog posts on CSDN, Juejin, Zhihu, and cnblogs.

### ImageJ Users

What they may care about:

- Familiar ImageJ-like workflow.
- Windows-native experience.
- ROI and measurement workflows.
- Clear list of supported and unsupported features.

Where to reach them:

- Image.sc forum.
- Scientific imaging communities.
- Microscopy-related groups.
- University lab communities.

### New Open-Source Contributors

What they may care about:

- Good first issues.
- Clear contribution guide.
- Small tasks.
- Friendly review.

Where to reach them:

- GitHub `good first issue`.
- Student developer communities.
- .NET beginner communities.

## GitHub Repository Setup

Recommended repository settings:

- Repository name: `ImageJCsharp`.
- Description: `C# native ImageJ-like scientific image analysis application for Windows.`
- Website: leave empty until there is a project page.
- Topics:
  - `imagej`
  - `csharp`
  - `dotnet`
  - `winforms`
  - `image-processing`
  - `scientific-imaging`
  - `microscopy`
  - `roi`
  - `open-source`

Enable:

- Issues.
- Discussions, if you want design and roadmap conversations.
- Actions.

## First Public Announcement

Suggested Chinese announcement:

```markdown
我正在开源一个新项目：ImageJCsharp。

它的长期目标是做一个 C# 原生的 ImageJ 替代软件，面向 Windows 和科研图像分析场景。

第一阶段不会追求完整复刻 ImageJ/Fiji 生态，而是先实现最常用、最可行的核心工作流：

- 打开和保存常见图片
- 图像查看、缩放、平移
- 矩形 ROI
- 面积、均值、最小值、最大值、标准差测量
- 阈值、二值化、边缘检测等基础处理
- 后续建立 C# 插件生态

项目刚起步，非常欢迎 C#、WinForms、图像处理、ImageJ 使用者、科研图像分析用户参与。

仓库地址：
<GitHub URL>
```

Suggested English announcement:

```markdown
I am starting an open-source project called ImageJCsharp.

The long-term goal is to build a C# native ImageJ-like scientific image analysis application for Windows.

The first stage focuses on feasible core workflows: opening images, viewing, rectangle ROI, measurements, thresholding, basic processing, and saving results. Java ImageJ plugin compatibility is not a goal. The future direction is a C#/.NET plugin ecosystem.

Contributors, testers, ImageJ users, and C# developers are welcome.

Repository:
<GitHub URL>
```

## Content Ideas

Good first articles:

- Why build a C# native ImageJ-like tool?
- What ImageJ workflows should be supported first?
- Building the first ROI measurement workflow in C#.
- Designing a C# plugin ecosystem for scientific image analysis.
- Comparing ImageJ behavior with ImageJCsharp MVP.

Good screenshots or GIFs:

- Open image.
- Zoom image.
- Draw rectangle ROI.
- Measure ROI.
- Apply threshold.
- Save result.

## Weekly Project Rhythm

Suggested rhythm:

- Once per week: publish a short progress update.
- Once per release: publish screenshots and release notes.
- Once per milestone: write a longer article.
- Continuously: label good first issues.

Weekly update template:

```markdown
ImageJCsharp weekly update

Done:
- 

Next:
- 

Need help:
- 

Known issues:
- 
```

## What To Avoid

- Do not promise full ImageJ/Fiji compatibility early.
- Do not claim scientific equivalence before tests exist.
- Do not overstate performance before benchmarks exist.
- Do not make the project look finished when it is early.
- Do not hide limitations; clear limitations build trust.
